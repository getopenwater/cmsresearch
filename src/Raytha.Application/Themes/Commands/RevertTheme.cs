using System.Text.Json;
using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.Commands;

public class RevertTheme
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public required ShortGuid ThemeId { get; init; }

        public static Command Empty() => new()
        {
            ThemeId = ShortGuid.Empty,
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x).Custom((request, context) =>
            {
                if (!db.ThemeRevisions.Any(rtr => rtr.Id == request.Id.Guid))
                    throw new NotFoundException("Theme Revision", request.Id);

                var theme = db.Themes.FirstOrDefault(t => t.Id == request.ThemeId.Guid);

                if (theme == null)
                    throw new NotFoundException("Theme", request.ThemeId);

                if (theme.IsActive)
                {
                    var defaultWebTemplates = new[]
                    {
                        BuiltInWebTemplate.HomePage.DeveloperName,
                        BuiltInWebTemplate.ContentItemDetailViewPage.DeveloperName,
                        BuiltInWebTemplate.ContentItemListViewPage.DeveloperName,
                    };

                    var webTemplateUsingInContentItem = db.ThemeWebTemplatesMappings
                        .Where(wtm => wtm.ThemeId == request.ThemeId.Guid)
                        .Where(wtm => wtm.ContentItemId != null)
                        .Include(wtm => wtm.WebTemplate)
                        .Select(wtm => wtm.WebTemplate)
                        .FirstOrDefault();

                    if (webTemplateUsingInContentItem != null && !defaultWebTemplates.Contains(webTemplateUsingInContentItem.DeveloperName))
                    {
                        context.AddFailure(Constants.VALIDATION_SUMMARY, $"The {webTemplateUsingInContentItem.Label} template is currently being used by content items. You must change the template those content items are using before reverting theme.");

                        return;
                    }

                    var webTemplateUsingInView = db.ThemeWebTemplatesMappings
                        .Where(wtm => wtm.ThemeId == request.ThemeId.Guid)
                        .Where(wtm => wtm.ViewId != null)
                        .Include(wtm => wtm.WebTemplate)
                        .Select(wtm => wtm.WebTemplate)
                        .FirstOrDefault();

                    if (webTemplateUsingInView != null && !defaultWebTemplates.Contains(webTemplateUsingInView.DeveloperName))
                    {
                        context.AddFailure(Constants.VALIDATION_SUMMARY, $"The {webTemplateUsingInView.Label} template is currently being used by list views. You must change the template those list views are using before reverting theme.");

                        return;
                    }
                }
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IRaythaDbContext _db;
        private readonly IMediator _mediator;

        public Handler(IRaythaDbContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var themeRevision = await _db.ThemeRevisions
                .Include(tr => tr.Theme)
                    .ThenInclude(t => t.WebTemplates)
                .FirstAsync(tr => tr.Id == request.Id.Guid, cancellationToken);

            var theme = themeRevision.Theme!;

            await _mediator.Send(new CreateThemeRevision.Command
            {
                ThemeId = theme.Id,
            }, cancellationToken);

            theme.Title = themeRevision.Title;
            theme.Description = themeRevision.Description;

            var webTemplatesInDb = theme.WebTemplates.ToDictionary(wt => wt.Id, wt => wt);
            var webTemplatesFromJson = JsonSerializer.Deserialize<ICollection<WebTemplate>>(themeRevision.WebTemplatesJson);
            var webTemplatesToAdd = new List<WebTemplate>();
            var webTemplatesToUpdate = new List<WebTemplate>();

            foreach (var webTemplateFromJson in webTemplatesFromJson!)
            {
                if (webTemplatesInDb.TryGetValue(webTemplateFromJson.Id, out var webTemplateInDb))
                {
                    webTemplateInDb.Label = webTemplateFromJson.Label;
                    webTemplateInDb.AllowAccessForNewContentTypes = webTemplateFromJson.AllowAccessForNewContentTypes;
                    webTemplateInDb.Content = webTemplateFromJson.Content;
                    webTemplateInDb.IsBaseLayout = webTemplateFromJson.IsBaseLayout;
                    webTemplateInDb.ParentTemplateId = webTemplateFromJson.ParentTemplateId;
                    webTemplateInDb.TemplateAccessToModelDefinitions = webTemplateFromJson.TemplateAccessToModelDefinitions;

                    webTemplatesToUpdate.Add(webTemplateInDb);
                }
                else
                {
                    webTemplatesToAdd.Add(webTemplateFromJson);
                }
            }

            _db.WebTemplates.UpdateRange(webTemplatesToUpdate);
            await _db.WebTemplates.AddRangeAsync(webTemplatesToAdd, cancellationToken);

            var webTemplatesToRemove = webTemplatesInDb.Values.Except(webTemplatesToUpdate).ToList();
            _db.WebTemplates.RemoveRange(webTemplatesToRemove);

            var webTemplatesMappingFromJson = JsonSerializer.Deserialize<ICollection<ThemeWebTemplatesMapping>>(themeRevision.WebTemplatesMappingJson);

            foreach (var webTemplateMappingFromJson in webTemplatesMappingFromJson!)
            {
                if (webTemplateMappingFromJson.ContentItemId != null)
                {
                    var contentItem = await _db.ContentItems
                        .FirstOrDefaultAsync(ci => ci.Id == webTemplateMappingFromJson.ContentItemId, cancellationToken);

                    if (contentItem != null)
                    {
                        contentItem.WebTemplateId = webTemplateMappingFromJson.WebTemplateId;

                        _db.ContentItems.Update(contentItem);
                    }
                }
                else if (webTemplateMappingFromJson.ViewId != null)
                {
                    var view = await _db.Views
                        .FirstOrDefaultAsync(v => v.Id == webTemplateMappingFromJson.ViewId, cancellationToken);

                    if (view != null)
                    {
                        view.WebTemplateId = webTemplateMappingFromJson.WebTemplateId;

                        _db.Views.Update(view);
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(themeRevision.ThemeId);
        }
    }
}