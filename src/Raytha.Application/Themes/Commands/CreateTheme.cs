using CSharpVitamins;
using FluentValidation;
using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Domain.Entities;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.Commands;

public class CreateTheme
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public required string Title { get; init; }
        public required string DeveloperName { get; init; }
        public required string Description { get; init; }
        public required bool InsertDefaultThemeMediaItems { get; init; }
        public string? ImageBase64 { get; init; }
        public string? ImageFileType { get; init; }
        public string? ImageFileName { get; init; }

        public static Command Empty() => new()
        {
            Title = string.Empty,
            DeveloperName = string.Empty,
            Description = string.Empty,
            InsertDefaultThemeMediaItems = false,
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.DeveloperName).NotEmpty();
            RuleFor(x => x).Custom((request, context) =>
            {
                if (db.Themes.Any(t => t.DeveloperName == request.DeveloperName.ToDeveloperName()))
                    context.AddFailure("DeveloperName", $"A theme with the developer name {request.DeveloperName.ToDeveloperName()} already exists.");
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
            var theme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                DeveloperName = request.DeveloperName,
                Description = request.Description,
            };

            await _db.Themes.AddAsync(theme, cancellationToken);

            theme.AddDomainEvent(new ThemeCreatedEvent(theme, request.ImageBase64, request.ImageFileName, request.ImageFileType, request.InsertDefaultThemeMediaItems));

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(theme.Id);
        }
    }
}