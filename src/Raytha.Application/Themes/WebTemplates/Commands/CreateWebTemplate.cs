﻿using CSharpVitamins;
using FluentValidation;
using MediatR;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Application.Themes.Commands;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.WebTemplates.Commands;

public class CreateWebTemplate
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public required ShortGuid ThemeId { get; init; }
        public required string DeveloperName { get; init; }
        public required string Label { get; init; }
        public required string Content { get; init; }
        public bool IsBaseLayout { get; init; }
        public ShortGuid? ParentTemplateId { get; init; }
        public bool AllowAccessForNewContentTypes { get; init; }
        public required IEnumerable<ShortGuid> TemplateAccessToModelDefinitions { get; init; }

        public static Command Empty() => new()
        {
            ThemeId = ShortGuid.Empty,
            Label = string.Empty,
            DeveloperName = string.Empty,
            Content = string.Empty,
            TemplateAccessToModelDefinitions = new List<ShortGuid>(),
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.ThemeId).NotEmpty();
            RuleFor(x => x).Custom((request, _) =>
            {
                if (!db.Themes.Any(t => t.Id == request.ThemeId.Guid))
                    throw new NotFoundException("Theme", request.ThemeId);
            });
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.DeveloperName).Must(StringExtensions.IsValidDeveloperName).WithMessage("Invalid developer name.");
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().Must(WebTemplateExtensions.HasRenderBodyTag).When(p => p.IsBaseLayout)
                .WithMessage("Content must have the {% renderbody %} tag if it is a base layout.");
            RuleFor(x => x).Custom((request, context) =>
            {
                var anyAlreadyExistWithDeveloperName = db.WebTemplates
                    .Where(wt => wt.ThemeId == request.ThemeId.Guid)
                    .Any(wt => wt.DeveloperName == request.DeveloperName.ToDeveloperName());

                if (anyAlreadyExistWithDeveloperName)
                    context.AddFailure("A web-template with that developer name already exists.");
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

            var entity = new WebTemplate
            {
                Id = Guid.NewGuid(),
                ThemeId = request.ThemeId.Guid,
                Label = request.Label,
                Content = request.Content,
                ParentTemplateId = request.ParentTemplateId.HasValue && request.ParentTemplateId != Guid.Empty ? request.ParentTemplateId : null,
                IsBaseLayout = request.IsBaseLayout,
                IsBuiltInTemplate = false,
                AllowAccessForNewContentTypes = request.AllowAccessForNewContentTypes,
                DeveloperName = request.DeveloperName.ToDeveloperName(),
                TemplateAccessToModelDefinitions = request.TemplateAccessToModelDefinitions?.Select(guid => new WebTemplateAccessToModelDefinition
                {
                    ContentTypeId = guid,
                }).ToList()
            };

            await _db.WebTemplates.AddAsync(entity, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}