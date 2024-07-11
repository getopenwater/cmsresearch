using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Domain.Entities;
using System.Text.Json;
using Raytha.Application.Themes.WebTemplates;

namespace Raytha.Application.Themes.Commands;

public class CreateThemeRevision
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
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
            RuleFor(x => x).Custom((request, _) =>
            {
                if (!db.Themes.Any(rt => rt.Id == request.ThemeId.Guid))
                    throw new NotFoundException("Theme", request.ThemeId);
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
                    .ThenInclude(wt => wt.TemplateAccessToModelDefinitions)
                .Include(t => t.WebTemplatesMappings)
                .FirstAsync(t => t.Id == request.ThemeId.Guid, cancellationToken);

            var entity = new ThemeRevision
            {
                ThemeId = theme.Id,
                Title = theme.Title,
                Description = theme.Description,
                WebTemplatesJson = JsonSerializer.Serialize(theme.WebTemplates.Select(WebTemplateJson.GetProjection)),
                WebTemplatesMappingJson = JsonSerializer.Serialize(theme.WebTemplatesMappings.Select(ThemeWebTemplatesMappingJson.GetProjection)),
            };

            await _db.ThemeRevisions.AddAsync(entity, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}