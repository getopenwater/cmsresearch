using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Domain.Entities;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.Commands;

public class DeleteTheme
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x).Custom((request, context) =>
            {
                var entity = db.Themes.FirstOrDefault(t => t.Id == request.Id.Guid);

                if (entity == null)
                {
                    throw new NotFoundException("Theme", request.Id);
                }

                if (entity.IsActive)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "You cannot delete an active theme. Set another theme as the active theme before deleting this one.");

                    return;
                }

                if (entity.DeveloperName == Theme.DEFAULT_THEME_DEVELOPER_NAME)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "You cannot delete the default theme.");

                    return;
                }
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var theme = await _db.Themes
                .Include(t => t.WebTemplates)
                .Include(t => t.WebTemplatesMappings)
                .FirstAsync(t => t.Id == request.Id.Guid, cancellationToken);

            _db.Themes.Remove(theme);

            theme.AddDomainEvent(new ThemeDeletedEvent(theme.Id));

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(theme.Id);
        }
    }
}