using System.Linq.Expressions;
using CSharpVitamins;
using Raytha.Application.Common.Models;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes;

public record ThemeRevisionDto : BaseAuditableEntityDto
{
    public required ShortGuid ThemeId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string WebTemplatesJson { get; init; }
    public AuditableUserDto? CreatorUser { get; init; }
    public AuditableUserDto? LastModifierUser { get; init; }

    public static Expression<Func<ThemeRevision, ThemeRevisionDto>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static ThemeRevisionDto GetProjection(ThemeRevision entity)
    {
        return new ThemeRevisionDto
        {
            Id = entity.Id,
            ThemeId = entity.ThemeId,
            Title = entity.Title,
            Description = entity.Description,
            WebTemplatesJson = entity.WebTemplatesJson,
            CreatorUser = AuditableUserDto.GetProjection(entity.CreatorUser),
            CreatorUserId = entity.CreatorUserId,
            CreationTime = entity.CreationTime,
            LastModifierUser = AuditableUserDto.GetProjection(entity.LastModifierUser),
            LastModifierUserId = entity.LastModifierUserId,
            LastModificationTime = entity.LastModificationTime,
        };
    }
}