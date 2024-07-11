using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.Commands;

public class SetAsActiveTheme
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x).Custom((request, _) =>
            {
                if (!db.Themes.Any(rt => rt.Id == request.Id.Guid))
                    throw new NotFoundException("Theme", request.Id);
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
            var previousActiveTheme = await _db.Themes
                .FirstOrDefaultAsync(rt => rt.IsActive, cancellationToken);

            if (previousActiveTheme != null)
                previousActiveTheme.IsActive = false;

            var entity = await _db.Themes
                .FirstAsync(rt => rt.Id == request.Id.Guid, cancellationToken);

            entity.IsActive = true;

            entity.AddDomainEvent(new ThemeChangedEvent(entity.Id));

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}