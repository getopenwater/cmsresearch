using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Application.Themes.Commands;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.WebTemplates.Commands;

public class EditWebTemplate
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public required ShortGuid ThemeId { get; init; }
        public string Label { get; init; } = null!;
        public string Content { get; init; } = null!;
        public bool IsBaseLayout { get; init; }
        public ShortGuid? ParentTemplateId { get; init; }
        public bool AllowAccessForNewContentTypes { get; init; }
        public IEnumerable<ShortGuid> TemplateAccessToModelDefinitions { get; init; } = null!;

        public static Command Empty() => new()
        {
            ThemeId = ShortGuid.Empty,
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.ThemeId).NotEmpty();
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().Must(WebTemplateExtensions.HasRenderBodyTag).When(p => p.IsBaseLayout)
                .WithMessage("Content must have the {% renderbody %} tag if it is a base layout.");
            RuleFor(x => x).Custom((request, context) =>
            {
                if (!db.Themes.Any(t => t.Id == request.ThemeId.Guid))
                    throw new NotFoundException("Theme", request.ThemeId.Guid);

                var entity = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .FirstOrDefault(p => p.Id == request.Id.Guid);

                if (entity == null)
                    throw new NotFoundException("Template", request.Id);

                var nonBaseLayoutsAllowedForNewTypes = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .Count(wt => !wt.IsBaseLayout && wt.AllowAccessForNewContentTypes);

                if (nonBaseLayoutsAllowedForNewTypes == 1)
                {
                    if (entity.Id == request.Id.Guid)
                    {
                        if (request.IsBaseLayout || !request.AllowAccessForNewContentTypes)
                        {
                            context.AddFailure("AllowAccessForNewContentTypes", "This is currently the only template that new content types can access. You must have at least 1 non base layout template new content types can default to.");
                        }
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
            var entity = await _db.WebTemplates
                .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                .Include(wt => wt.TemplateAccessToModelDefinitions)
                .FirstAsync(wt => wt.Id == request.Id.Guid, cancellationToken);

            if (request.ParentTemplateId.HasValue && request.ParentTemplateId != Guid.Empty)
            {
                var parent = await _db.WebTemplates
                    .FirstAsync(wt => wt.Id == request.ParentTemplateId.Value.Guid, cancellationToken);

                if (parent.ParentTemplateId != null)
                    await WebTemplateUtility.LoadParentWebTemplatesRecursiveAsync(parent, _db, cancellationToken);

                var iterator = parent;
                while (iterator != null)
                {
                    if (iterator.Id == entity.Id)
                        throw new BusinessException("A circular dependency was detected with this base layout relationship.");

                    iterator = iterator.ParentTemplate;
                }
            }

            if (!request.IsBaseLayout && entity.IsBaseLayout)
            {
                var hasChildTemplates = _db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .Any(wt => wt.ParentTemplateId == entity.Id);

                if (hasChildTemplates)
                    throw new BusinessException("This template has other templates that inherit from it.");
            }

            await _mediator.Send(new CreateThemeRevision.Command
            {
                ThemeId = request.ThemeId,
            }, cancellationToken);

            var revision = new WebTemplateRevision
            {
                WebTemplateId = entity.Id,
                Content = entity.Content,
                Label = entity.Label,
                AllowAccessForNewContentTypes = entity.AllowAccessForNewContentTypes
            };

            _db.WebTemplateRevisions.Add(revision);

            entity.Label = request.Label;
            entity.Content = request.Content;
            entity.ParentTemplateId = request.ParentTemplateId.HasValue && request.ParentTemplateId != Guid.Empty ? request.ParentTemplateId : null;
            if (!entity.IsBuiltInTemplate)
            {
                entity.IsBaseLayout = request.IsBaseLayout;
            }
            entity.AllowAccessForNewContentTypes = request.AllowAccessForNewContentTypes;

            var accessToRemoveItemGuids = entity.TemplateAccessToModelDefinitions
                .Select(md => (ShortGuid)md.Id)
                .Except(request.TemplateAccessToModelDefinitions);

            var accessToRemoveItems = entity.TemplateAccessToModelDefinitions?.Where(md => accessToRemoveItemGuids.Contains(md.Id));
            var accessToAddItemGuids = request.TemplateAccessToModelDefinitions?.Except(entity.TemplateAccessToModelDefinitions?.ToList().Select(p => (ShortGuid)p.Id) ?? new List<ShortGuid>());

            if (accessToRemoveItems != null)
            {
                _db.WebTemplateAccessToModelDefinitions.RemoveRange(accessToRemoveItems);
            }

            if (accessToAddItemGuids != null)
            {
                _db.WebTemplateAccessToModelDefinitions.AddRange(accessToAddItemGuids.Select(guid => new WebTemplateAccessToModelDefinition
                {
                    WebTemplateId = entity.Id,
                    ContentTypeId = guid
                }));
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(request.Id);
        }
    }
}
