using CSharpVitamins;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using System.Text.Json;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.Commands;

public class SetAsActiveTheme
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public required IDictionary<string, string>? MatchedWebTemplateDeveloperNames { get; init; }

        public static Command Empty() => new()
        {
            MatchedWebTemplateDeveloperNames = new Dictionary<string, string>(),
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x).Custom((request, context) =>
            {
                if (!db.Themes.Any(rt => rt.Id == request.Id.Guid))
                    throw new NotFoundException("Theme", request.Id);

                if (request.MatchedWebTemplateDeveloperNames?.Count > 0)
                {
                    var activeThemeId = db.OrganizationSettings
                        .Select(os => os.ActiveThemeId)
                        .First();

                    var activeThemeWebTemplates = db.WebTemplates
                        .Where(wt => wt.ThemeId == activeThemeId)
                        .Select(wt => wt.DeveloperName)
                        .ToList();

                    var newActiveThemeWebTemplates = db.WebTemplates
                        .Where(wt => wt.ThemeId == request.Id.Guid)
                        .Select(wt => wt.DeveloperName)
                        .ToList();

                    foreach (var matchedTemplate in request.MatchedWebTemplateDeveloperNames)
                    {
                        if (!activeThemeWebTemplates.Contains(matchedTemplate.Key))
                        {
                            context.AddFailure($"The template '{matchedTemplate.Key}' from the active theme was not found.");
                        }

                        if (!newActiveThemeWebTemplates.Contains(matchedTemplate.Value))
                        {
                            context.AddFailure($"The template '{matchedTemplate.Value}' from the current theme was not found.");
                        }
                    }
                }
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IBackgroundTaskQueue _taskQueue;

        public Handler(IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var backgroundJobId = await _taskQueue.EnqueueAsync<BackgroundTask>(request, cancellationToken);

            return new CommandResponseDto<ShortGuid>(backgroundJobId);
        }
    }

    public class BackgroundTask : IExecuteBackgroundTask
    {
        private readonly IRaythaDbContext _db;

        public BackgroundTask(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task Execute(Guid jobId, JsonElement args, CancellationToken cancellationToken)
        {
            var themeId = args.GetProperty("Id").GetProperty("Guid").GetGuid();
            var matchedWebTemplates = JsonSerializer.Deserialize<IDictionary<string, string>>(args.GetProperty("MatchedWebTemplateDeveloperNames").GetRawText());

            var job = _db.BackgroundTasks.First(p => p.Id == jobId);

            job.TaskStep = 1;
            job.StatusInfo = "Setting the theme as active";
            job.PercentComplete = 0;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);

            if (matchedWebTemplates?.Count > 0)
            {
                var previousActiveThemeId = await _db.OrganizationSettings
                    .Select(os => os.ActiveThemeId)
                    .FirstAsync(cancellationToken);

                var contentItemIds = await _db.ContentItems.Select(ci => ci.Id).ToListAsync(cancellationToken);

                foreach (var contentItemId in contentItemIds)
                {
                    var anyWebTemplateContentItemMapping = await _db.ThemeWebTemplateContentItemMappings
                        .Where(wtm => wtm.ThemeId == themeId)
                        .AnyAsync(wtm => wtm.ContentItemId == contentItemId, cancellationToken);

                    if (!anyWebTemplateContentItemMapping)
                    {
                        var previousThemeWebTemplateDeveloperName = await _db.ThemeWebTemplateContentItemMappings
                            .Where(wtm => wtm.ThemeId == previousActiveThemeId)
                            .Where(wtm => wtm.ContentItemId == contentItemId)
                            .Select(wtm => wtm.WebTemplate!.DeveloperName)
                            .FirstAsync(cancellationToken);

                        var matchedWebTemplateDeveloperName = matchedWebTemplates[previousThemeWebTemplateDeveloperName!];

                        var webTemplateId = await _db.WebTemplates
                            .Where(wt => wt.ThemeId == themeId)
                            .Where(wt => wt.DeveloperName == matchedWebTemplateDeveloperName)
                            .Select(wt => wt.Id)
                            .FirstAsync(cancellationToken);

                        var themeWebTemplateContentMapping = new ThemeWebTemplateContentItemMapping
                        {
                            Id = Guid.NewGuid(),
                            ThemeId = themeId,
                            ContentItemId = contentItemId,
                            WebTemplateId = webTemplateId,
                        };

                        _db.ThemeWebTemplateContentItemMappings.Add(themeWebTemplateContentMapping);
                    }
                }

                job.TaskStep = 2;
                job.StatusInfo = "Templates for content items have been updated.";
                job.PercentComplete = 40;
                _db.BackgroundTasks.Update(job);
                await _db.SaveChangesAsync(cancellationToken);

                var viewIds = await _db.Views
                    .Select(v => v.Id)
                    .ToListAsync(cancellationToken);

                foreach (var viewId in viewIds)
                {
                    var anyWebTemplateViewMapping = await _db.ThemeWebTemplateViewMappings
                        .Where(wtm => wtm.ThemeId == themeId)
                        .AnyAsync(wtm => wtm.ViewId == viewId, cancellationToken);

                    if (!anyWebTemplateViewMapping)
                    {
                        var previousThemeWebTemplateDeveloperName = await _db.ThemeWebTemplateViewMappings
                            .Where(wtm => wtm.ThemeId == previousActiveThemeId)
                            .Where(wtm => wtm.ViewId == viewId)
                            .Select(wtm => wtm.WebTemplate!.DeveloperName)
                            .FirstAsync(cancellationToken);

                        var matchedWebTemplateDeveloperName = matchedWebTemplates[previousThemeWebTemplateDeveloperName!];

                        var webTemplateId = await _db.WebTemplates
                            .Where(wt => wt.ThemeId == themeId)
                            .Where(wt => wt.DeveloperName == matchedWebTemplateDeveloperName)
                            .Select(wt => wt.Id)
                            .FirstAsync(cancellationToken);

                        var themeWebTemplateContentMapping = new ThemeWebTemplateViewMapping
                        {
                            Id = Guid.NewGuid(),
                            ThemeId = themeId,
                            ViewId = viewId,
                            WebTemplateId = webTemplateId,
                        };

                        _db.ThemeWebTemplateViewMappings.Add(themeWebTemplateContentMapping);
                    }
                }

                job.TaskStep = 3;
                job.StatusInfo = "Templates for views have been updated.";
                job.PercentComplete = 80;
                _db.BackgroundTasks.Update(job);
                await _db.SaveChangesAsync(cancellationToken);
            }

            var organizationSettings = await _db.OrganizationSettings
                .FirstAsync(cancellationToken);

            organizationSettings.ActiveThemeId = themeId;
            _db.OrganizationSettings.Update(organizationSettings);
            await _db.SaveChangesAsync(cancellationToken);

            job.TaskStep = 4;
            job.StatusInfo = "Setting the theme as an active theme is complete.";
            job.PercentComplete = 100;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);
            Thread.Sleep(1000);
        }
    }
}