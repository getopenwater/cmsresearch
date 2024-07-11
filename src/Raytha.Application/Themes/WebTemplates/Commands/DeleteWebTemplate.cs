using CSharpVitamins;
using FluentValidation;
using MediatR;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Application.Themes.Commands;

namespace Raytha.Application.Themes.WebTemplates.Commands;

public class DeleteWebTemplate
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
            RuleFor(x => x.ThemeId).NotEmpty();
            RuleFor(x => x).Custom((request, context) =>
            {
                if (!db.Themes.Any(t => t.Id == request.ThemeId.Guid))
                    throw new NotFoundException("Theme", request.ThemeId.Guid);

                var entity = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .FirstOrDefault(p => p.Id == request.Id.Guid);

                if (entity == null)
                    throw new NotFoundException("Template", request.Id);

                var anyContentItemsUsingTemplate = db.ContentItems.Any(ci => ci.WebTemplateId == entity.Id);
                if (anyContentItemsUsingTemplate)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "This template is currently being used by content items. You must change the template those content items are using before deleting this one.");
                    return;
                }

                var anyViewsUsingTemplate = db.Views.Any(v => v.WebTemplateId == entity.Id);
                if (anyViewsUsingTemplate)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "This template is currently being used by list views. You must change the template those list views are using before deleting this one.");
                    return;
                }

                if (entity.IsBuiltInTemplate)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "You cannot remove built-in templates.");
                    return;
                }

                var hasChildTemplates = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .Any(p => p.ParentTemplateId == entity.Id);

                if (hasChildTemplates)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "You must first remove or re-assign child templates.");
                    return;
                }

                var nonBaseLayoutsAllowedForNewTypes = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .Count(wt => !wt.IsBaseLayout && wt.AllowAccessForNewContentTypes);

                if (nonBaseLayoutsAllowedForNewTypes == 1)
                {
                    if (entity.Id == request.Id.Guid)
                    {
                        context.AddFailure("AllowAccessForNewContentTypes", "This is currently the only template that new content types can access. You must have at least 1 non base layout template new content types can default to.");
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
            await _mediator.Send(new CreateThemeRevision.Command
            {
                ThemeId = request.ThemeId,
            }, cancellationToken);

            var entity = _db.WebTemplates
                .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                .First(p => p.Id == request.Id.Guid);

            _db.WebTemplates.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
