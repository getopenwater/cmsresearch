using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;

namespace Raytha.Application.Themes.Commands;

public class EditThemeForExport
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public required bool IsCanExport { get; init; }

        public static Command Empty() => new()
        {
            IsCanExport = false,
        };
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
            var entity = await _db.Themes
                .FirstAsync(rt => rt.Id == request.Id.Guid, cancellationToken);

            entity.IsCanExport = request.IsCanExport;

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}