using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpVitamins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raytha.Application.BackgroundTasks.Queries;
using Raytha.Application.Common.Utils;
using Raytha.Application.ContentTypes;
using Raytha.Application.ContentTypes.Queries;
using Raytha.Application.MediaItems.Commands;
using Raytha.Application.Themes.Commands;
using Raytha.Application.Themes.MediaItems.Queries;
using Raytha.Application.Themes.Queries;
using Raytha.Application.Themes.WebTemplates;
using Raytha.Application.Themes.WebTemplates.Commands;
using Raytha.Application.Themes.WebTemplates.Queries;
using Raytha.Domain.Entities;
using Raytha.Domain.ValueObjects;
using Raytha.Domain.ValueObjects.FieldTypes;
using Raytha.Web.Areas.Admin.Views.Shared.ViewModels;
using Raytha.Web.Areas.Admin.Views.Themes;
using Raytha.Web.Areas.Admin.Views.Themes.MediaItems;
using Raytha.Web.Areas.Admin.Views.Themes.WebTemplates;
using Raytha.Web.Filters;
using Raytha.Web.Utils;

namespace Raytha.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = BuiltInSystemPermission.MANAGE_SYSTEM_SETTINGS_PERMISSION)]
public class ThemesController : BaseController
{
    [ServiceFilter(typeof(SetPaginationInformationFilterAttribute))]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes", Name = "themesindex")]
    public async Task<IActionResult> Index(string search = "",
                                           string orderBy = $"CreationTime {SortOrder.DESCENDING}",
                                           int pageNumber = 1,
                                           int pageSize = 50)
    {
        var input = new GetThemes.Query
        {
            Search = search,
            OrderBy = orderBy,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var allThemeResponse = await Mediator.Send(input);
        var latestThemeRevisionsResponse = await Mediator.Send(new GetLatestThemeRevisions.Query());

        var items = allThemeResponse.Result.Items.Select(t =>
        {
            var latestRevision = latestThemeRevisionsResponse.Result.FirstOrDefault(tr => tr.ThemeId == t.Id);
            var (lastModificationTime, lastModificationUser) = Nullable.Compare(t.LastModificationTime, latestRevision?.CreationTime) > 0
                    ? (t.LastModificationTime, t.LastModifierUser)
                    : (latestRevision?.CreationTime, latestRevision?.CreatorUser);

            return new ThemesListItem_ViewModel
            {
                Id = t.Id,
                Title = t.Title,
                DeveloperName = t.DeveloperName,
                Description = t.Description,
                IsActive = t.IsActive,
                PreviewImageObjectKey = t.PreviewImageMediaItem?.ObjectKey,
                LastModificationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(lastModificationTime),
                LastModifierUser = lastModificationUser?.FullName ?? "N/A",
            };
        });

        var viewModel = new List_ViewModel<ThemesListItem_ViewModel>(items, allThemeResponse.Result.TotalCount);

        return View(viewModel);
    }

    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/create", Name = "themescreate")]
    public IActionResult Create()
    {
        return View(new ThemesCreate_ViewModel
        {
            UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud,
            PathBase = CurrentOrganization.PathBase,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/create", Name = "themescreate")]
    public async Task<IActionResult> Create(ThemesCreate_ViewModel model)
    {
        var input = new CreateTheme.Command
        {
            Title = model.Title,
            DeveloperName = model.DeveloperName,
            Description = model.Description,
            InsertDefaultThemeMediaItems = model.InsertDefaultThemeMediaItems,
            ImageBase64 = model.ImageBase64,
            ImageFileName = model.ImageFileName,
            ImageFileType = model.ImageFileType,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage($"{model.Title} was created successfully.");

            return RedirectToAction(nameof(Edit), new { id = response.Result.Guid });
        }
        else
        {
            SetErrorMessage("There was an error attempting to create this theme. See the error below.", response.GetErrors());

            model.UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud;
            model.PathBase = CurrentOrganization.PathBase;

            return View(model);
        }
    }

    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}", Name = "themesedit")]
    public async Task<IActionResult> Edit(string id)
    {
        var input = new GetThemeById.Query
        {
            Id = id
        };

        var response = await Mediator.Send(input);

        var model = new ThemesEdit_ViewModel
        {
            Id = id,
            Title = response.Result.Title,
            DeveloperName = response.Result.DeveloperName,
            Description = response.Result.Description,
            PreviewImageObjectKey = response.Result.PreviewImageMediaItem?.ObjectKey,
            IsActive = response.Result.IsActive,
            UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud,
            PathBase = CurrentOrganization.PathBase,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}", Name = "themesedit")]
    public async Task<IActionResult> Edit(string id, ThemesEdit_ViewModel model)
    {
        var input = new EditTheme.Command
        {
            Id = id,
            Title = model.Title,
            Description = model.Description,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage($"{model.Title} was updated successfully.");

            return RedirectToAction(nameof(Edit), new { id });
        }
        else
        {
            SetErrorMessage("There was an error attempting to update this theme. See the error below.", response.GetErrors());

            model.UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud;
            model.PathBase = CurrentOrganization.PathBase;

            return View(model);
        }
    }

    [HttpPost]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/delete/{{id}}", Name = "themesdelete")]
    public async Task<IActionResult> Delete(string id)
    {
        var input = new DeleteTheme.Command
        {
            Id = id
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage("Theme has been deleted.");

            return RedirectToAction(nameof(Index));
        }
        else
        {
            SetErrorMessage("There was an error deleting this theme", response.GetErrors());

            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}/revisions", Name = "themesrevisionsindex")]
    public async Task<IActionResult> Revisions(string id,
                                               string orderBy = $"CreationTime {SortOrder.DESCENDING}",
                                               int pageNumber = 1,
                                               int pageSize = 50)
    {
        var input = new GetThemeRevisionsByThemeId.Query
        {
            ThemeId = id,
            OrderBy = orderBy,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var response = await Mediator.Send(input);

        var items = response.Result.Items.Select(tr => new ThemesRevisionsListItem_ViewModel
        {
            Id = tr.Id,
            Title = tr.Title,
            Description = tr.Description,
            WebTemplates = tr.WebTemplatesJson,
            CreationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(tr.CreationTime),
            CreatorUser = tr.CreatorUser?.FullName ?? "N/A",
        });

        var viewModel = new ThemeRevisionsPagination_ViewModel(items, response.Result.TotalCount)
        {
            ThemeId = id,
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}/revisions/{{revisionId}}", Name = "themesrevisionsrevert")]
    public async Task<IActionResult> RevisionRevert(string id, string revisionId)
    {
        var input = new RevertTheme.Command
        {
            Id = revisionId,
            ThemeId = id,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage("Theme has been reverted.");

            return RedirectToAction(nameof(Edit), new { id });
        }
        else
        {
            SetErrorMessage("There was an error reverting this theme", response.GetErrors());

            return RedirectToAction(nameof(Revisions), new { id });
        }
    }

    [HttpPost]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/set-as-active-theme/{{id}}", Name = "themessetasactivetheme")]
    public async Task<IActionResult> SetAsActiveTheme(string id)
    {
        var input = new SetAsActiveTheme.Command
        {
            Id = id,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
            SetSuccessMessage("The theme has been successfully set as active.");
        else
            SetErrorMessage("There was an error setting this as the active theme.", response.GetErrors());

        return RedirectToAction(nameof(Edit), new { id });
    }

    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}/export", Name = "themesexporttourl")]
    public async Task<IActionResult> BeginExportToUrl(string id)
    {
        var response = await Mediator.Send(new GetThemeById.Query
        {
            Id = id,
        });

        return View(new ThemesBeginExportToUrl_ViewModel
        {
            Id = id,
            IsCanExport = response.Result.IsCanExport,
            Url = $"{CurrentOrganization.WebsiteUrl}{RAYTHA_ROUTE_PREFIX}/themes/export/{response.Result.DeveloperName}",
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{id}}/export", Name = "themesexporttourl")]
    public async Task<IActionResult> BeginExportToUrl(ThemesBeginExportToUrl_ViewModel model, string id)
    {
        var input = new EditThemeForExport.Command
        {
            Id = id,
            IsCanExport = model.IsCanExport,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
            SetSuccessMessage($"The current theme {(model.IsCanExport ? "can" : "cannot")} be exported");
        else
            SetErrorMessage("An error occurred while saving. See the error below.", response.GetErrors());

        return RedirectToAction(nameof(BeginExportToUrl), new { id });
    }

    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/import/", Name = "themesimportfromurl")]
    public async Task<IActionResult> BeginImportFromUrl()
    {
        return View(new ThemesBeginImportFromUrl_ViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/import/", Name = "themesimportfromurl")]
    public async Task<IActionResult> BeginImportFromUrl(ThemesBeginImportFromUrl_ViewModel model, string url)
    {
        var input = new BeginImportThemeFromUrl.Command
        {
            Url = url,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage("Import in progress.");

            return RedirectToAction(nameof(BackgroundTaskStatus), new { id = response.Result });
        }
        else
        {
            SetErrorMessage("There was an error attempting while importing. See the error below.", response.GetErrors());

            return View(model);
        }
    }

    [Authorize(Policy = BuiltInContentTypePermission.CONTENT_TYPE_READ_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/background-task/status/{{id}}", Name = "themesbackgroundtaskstatus")]
    public async Task<IActionResult> BackgroundTaskStatus(string id, bool json = false)
    {
        var response = await Mediator.Send(new GetBackgroundTaskById.Query
        {
            Id = id
        });

        var model = new ThemesBackgroundTaskStatus_ViewModel
        {
            PathBase = CurrentOrganization.PathBase
        };

        return json
            ? Ok(response.Result)
            : View(model);
    }

    [AllowAnonymous]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/export/{{developerName}}")]
    public async Task<IActionResult> ExportTheme(string developerName)
    {
        var input = new ExportTheme.Command
        {
            DeveloperName = developerName,
        };

        var response = await Mediator.Send(input);

        return Json(response.Result, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }

    [ServiceFilter(typeof(SetPaginationInformationFilterAttribute))]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates", Name = "webtemplatesindex")]
    public async Task<IActionResult> WebTemplatesIndex(string themeId,
                                                       string orderBy = $"Label {SortOrder.ASCENDING}",
                                                       string search = "",
                                                       int pageNumber = 1,
                                                       int pageSize = 50)
    {
        var input = new GetWebTemplates.Query
        {
            ThemeId = themeId,
            OrderBy = orderBy,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var response = await Mediator.Send(input);

        var items = response.Result.Items.Select(p => new WebTemplatesListItem_ViewModel
        {
            Id = p.Id,
            Label = p.Label,
            DeveloperName = p.DeveloperName,
            LastModificationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(p.LastModificationTime),
            LastModifierUser = p.LastModifierUser != null ? p.LastModifierUser.FullName : "N/A",
            IsBuiltInTemplate = p.IsBuiltInTemplate.YesOrNo(),
            ParentTemplate = p.ParentTemplate != null
                ? new WebTemplatesListItem_ViewModel.ParentTemplate_ViewModel { Id = p.ParentTemplate.Id, Label = p.ParentTemplate.Label }
                : null
        });

        var viewModel = new WebTemplatesPagination_ViewModel(items, response.Result.TotalCount, themeId);

        return View("~/Areas/Admin/Views/Themes/WebTemplates/Index.cshtml", viewModel);
    }

    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/create", Name = "webtemplatescreate")]
    public async Task<IActionResult> WebTemplatesCreate(string themeId)
    {
        var webTemplatesResponse = await Mediator.Send(new GetWebTemplates.Query
        {
            ThemeId = themeId,
            PageSize = int.MaxValue,
            BaseLayoutsOnly = true
        });

        var contentTypesResponse = await Mediator.Send(new GetContentTypes.Query
        {
            PageSize = int.MaxValue
        });

        var mediaItemsResponse = await Mediator.Send(new GetMediaItemsByThemeId.Query
        {
            ThemeId = themeId,
        });

        var templateAccessList = contentTypesResponse.Result.Items.Select(p => new WebTemplateAccessToModelDefinitions_ViewModel
        {
            Id = p.Id,
            Key = p.LabelPlural,
            Value = true
        });

        var templateVariableDictionary = GetInsertVariablesViewModel(string.Empty, false, contentTypesResponse.Result.Items);

        var viewModel = new WebTemplatesCreate_ViewModel
        {
            ThemeId = themeId,
            ParentTemplates = webTemplatesResponse.Result.Items,
            TemplateAccessToModelDefinitions = templateAccessList.ToArray(),
            TemplateVariables = templateVariableDictionary,
            AllowedMimeTypes = FileStorageProviderSettings.AllowedMimeTypes,
            MaxFileSize = FileStorageProviderSettings.MaxFileSize,
            UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud,
            PathBase = CurrentOrganization.PathBase,
            MediaItems = mediaItemsResponse.Result,
        };

        return View("~/Areas/Admin/Views/Themes/WebTemplates/Create.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/create", Name = "webtemplatescreate")]
    public async Task<IActionResult> WebTemplatesCreate(WebTemplatesCreate_ViewModel model, string themeId)
    {
        var input = new CreateWebTemplate.Command
        {
            ThemeId = themeId,
            Label = model.Label,
            Content = model.Content,
            ParentTemplateId = model.ParentTemplateId,
            IsBaseLayout = model.IsBaseLayout,
            DeveloperName = model.DeveloperName,
            TemplateAccessToModelDefinitions = model.TemplateAccessToModelDefinitions.Where(p => p.Value).Select(p => (ShortGuid)p.Id),
            AllowAccessForNewContentTypes = model.AllowAccessForNewContentTypes,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage($"{model.Label} was created successfully.");

            return RedirectToAction(nameof(WebTemplatesEdit), new { themeId, id = response.Result });
        }
        else
        {

            var baseLayouts = await Mediator.Send(new GetWebTemplates.Query
            {
                ThemeId = themeId,
                PageSize = int.MaxValue,
                BaseLayoutsOnly = true
            });

            model.ParentTemplates = baseLayouts.Result.Items;

            var contentTypes = await Mediator.Send(new GetContentTypes.Query { PageSize = int.MaxValue });
            var templateVariableDictionary = GetInsertVariablesViewModel(model.DeveloperName, false, contentTypes.Result.Items);
            model.TemplateVariables = templateVariableDictionary;

            var mediaItemsResponse = await Mediator.Send(new GetMediaItemsByThemeId.Query
            {
                ThemeId = themeId,
            });

            model.MediaItems = mediaItemsResponse.Result;

            model.ThemeId = themeId;
            model.AllowedMimeTypes = FileStorageProviderSettings.AllowedMimeTypes;
            model.MaxFileSize = FileStorageProviderSettings.MaxFileSize;
            model.UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud;
            model.PathBase = CurrentOrganization.PathBase;

            SetErrorMessage("There was an error attempting to update this template. See the error below.", response.GetErrors());

            return View("~/Areas/Admin/Views/Themes/WebTemplates/Create.cshtml", model);
        }
    }

    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/edit/{{id}}", Name = "webtemplatesedit")]
    public async Task<IActionResult> WebTemplatesEdit(string themeId, string id)
    {
        var webTemplateResponse = await Mediator.Send(new GetWebTemplateById.Query { Id = id });

        var baseLayouts = await Mediator.Send(new GetWebTemplates.Query
        {
            ThemeId = themeId,
            PageSize = int.MaxValue,
            BaseLayoutsOnly = true
        });

        var childLayouts = GetChildren(baseLayouts.Result.Items.ToArray(), webTemplateResponse.Result);
        var lineage = childLayouts.Union(new List<WebTemplateDto>() { webTemplateResponse.Result });
        var excepted = baseLayouts.Result.Items.Select(p => p.DeveloperName).Except(lineage.Select(p => p.DeveloperName));
        var baseLayoutsDictionary = baseLayouts.Result.Items.Where(p => excepted.Contains(p.DeveloperName)).ToDictionary(k => k.Id.ToString(), v => v.Label);

        var contentTypes = await Mediator.Send(new GetContentTypes.Query());
        var templateAccessChoiceItems = new List<WebTemplateAccessToModelDefinitions_ViewModel>();
        foreach (var contentType in contentTypes.Result.Items)
        {
            templateAccessChoiceItems.Add(new WebTemplateAccessToModelDefinitions_ViewModel
            {
                Id = contentType.Id,
                Key = contentType.LabelPlural,
                Value = webTemplateResponse.Result.TemplateAccessToModelDefinitions.ContainsKey(contentType.Id)
            });
        }

        var templateVariableDictionary = GetInsertVariablesViewModel(webTemplateResponse.Result.DeveloperName, webTemplateResponse.Result.IsBuiltInTemplate, contentTypes.Result.Items);

        var mediaItemsResponse = await Mediator.Send(new GetMediaItemsByThemeId.Query
        {
            ThemeId = themeId,
        });

        var model = new WebTemplatesEdit_ViewModel
        {
            Id = webTemplateResponse.Result.Id,
            Content = webTemplateResponse.Result.Content,
            Label = webTemplateResponse.Result.Label,
            DeveloperName = webTemplateResponse.Result.DeveloperName,
            ParentTemplateId = webTemplateResponse.Result.ParentTemplateId,
            ParentTemplates = baseLayoutsDictionary,
            IsBaseLayout = webTemplateResponse.Result.IsBaseLayout,
            IsBuiltInTemplate = webTemplateResponse.Result.IsBuiltInTemplate,
            TemplateAccessToModelDefinitions = templateAccessChoiceItems.ToArray(),
            TemplateVariables = templateVariableDictionary,
            AllowAccessForNewContentTypes = webTemplateResponse.Result.AllowAccessForNewContentTypes,
            AllowedMimeTypes = FileStorageProviderSettings.AllowedMimeTypes,
            MaxFileSize = FileStorageProviderSettings.MaxFileSize,
            UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud,
            PathBase = CurrentOrganization.PathBase,
            ThemeId = themeId,
            MediaItems = mediaItemsResponse.Result,
        };

        if (WebTemplateExtensions.HasRenderBodyTag(webTemplateResponse.Result.Content) && !webTemplateResponse.Result.IsBaseLayout)
            SetWarningMessage("{% renderbody %} is present and this template is not a base layout. This may result in a rendering error or crash if not handled properly.");

        return View("~/Areas/Admin/Views/Themes/WebTemplates/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/edit/{{id}}", Name = "webtemplatesedit")]
    public async Task<IActionResult> WebTemplatesEdit(WebTemplatesEdit_ViewModel model, string themeId, string id)
    {
        var input = new EditWebTemplate.Command
        {
            Id = id,
            ThemeId = themeId,
            Label = model.Label,
            Content = model.Content,
            ParentTemplateId = model.ParentTemplateId,
            IsBaseLayout = model.IsBaseLayout,
            AllowAccessForNewContentTypes = model.AllowAccessForNewContentTypes,
            TemplateAccessToModelDefinitions = model.TemplateAccessToModelDefinitions.Where(p => p.Value).Select(p => (ShortGuid)p.Id)
        };
        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage($"{model.Label} was updated successfully.");
            return RedirectToAction(nameof(WebTemplatesEdit), new { themeId, id });
        }
        else
        {
            var templateResponse = await Mediator.Send(new GetWebTemplateById.Query { Id = id });

            var baseLayouts = await Mediator.Send(new GetWebTemplates.Query
            {
                ThemeId = themeId,
                PageSize = int.MaxValue,
                BaseLayoutsOnly = true
            });

            var childLayouts = GetChildren(baseLayouts.Result.Items.ToArray(), templateResponse.Result);
            var lineage = childLayouts.Union(new List<WebTemplateDto>() { templateResponse.Result });
            var excepted = baseLayouts.Result.Items.Select(p => p.DeveloperName).Except(lineage.Select(p => p.DeveloperName));
            var baseLayoutsDictionary = baseLayouts.Result.Items.Where(p => excepted.Contains(p.DeveloperName)).ToDictionary(k => k.Id.ToString(), v => v.Label);
            model.ParentTemplates = baseLayoutsDictionary;

            var contentTypes = await Mediator.Send(new GetContentTypes.Query { PageSize = int.MaxValue });
            var templateVariableDictionary = GetInsertVariablesViewModel(templateResponse.Result.DeveloperName, templateResponse.Result.IsBuiltInTemplate, contentTypes.Result.Items);
            model.TemplateVariables = templateVariableDictionary;

            var mediaItemsResponse = await Mediator.Send(new GetMediaItemsByThemeId.Query
            {
                ThemeId = themeId,
            });

            model.MediaItems = mediaItemsResponse.Result;

            model.ThemeId = themeId;
            model.AllowedMimeTypes = FileStorageProviderSettings.AllowedMimeTypes;
            model.MaxFileSize = FileStorageProviderSettings.MaxFileSize;
            model.UseDirectUploadToCloud = FileStorageProviderSettings.UseDirectUploadToCloud;
            model.PathBase = CurrentOrganization.PathBase;

            SetErrorMessage("There was an error attempting to update this template. See the error below.", response.GetErrors());

            return View("~/Areas/Admin/Views/Themes/WebTemplates/Edit.cshtml", model);
        }
    }

    [HttpPost]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/delete/{{id}}", Name = "webtemplatesdelete")]
    public async Task<IActionResult> WebTemplatesDelete(string themeId, string id)
    {
        var input = new DeleteWebTemplate.Command
        {
            Id = id,
            ThemeId = themeId,
        };

        var response = await Mediator.Send(input);
        if (response.Success)
        {
            SetSuccessMessage("Web-template has been deleted.");

            return RedirectToAction(nameof(WebTemplatesIndex), new { themeId });
        }
        else
        {
            SetErrorMessage("There was an error deleting this web-template", response.GetErrors());

            return RedirectToAction(nameof(WebTemplatesEdit), new { themeId, id, });
        }
    }

    [ServiceFilter(typeof(SetPaginationInformationFilterAttribute))]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/edit/{{id}}/revisions", Name = "webtemplatesrevisionsindex")]
    public async Task<IActionResult> Revisions(string themeId,
                                               string id,
                                               string orderBy = $"CreationTime {SortOrder.DESCENDING}",
                                               int pageNumber = 1,
                                               int pageSize = 50)
    {
        var template = await Mediator.Send(new GetWebTemplateById.Query { Id = id });

        var input = new GetWebTemplateRevisionsByTemplateId.Query
        {
            Id = id,
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = orderBy,
        };

        var response = await Mediator.Send(input);
        var items = response.Result.Items.Select(p => new WebTemplatesRevisionsListItem_ViewModel
        {
            Id = p.Id,
            Label = p.Label,
            CreationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(p.CreationTime),
            CreatorUser = p.CreatorUser != null ? p.CreatorUser.FullName : "N/A",
            Content = p.Content
        });

        var viewModel = new WebTemplatesRevisionsPagination_ViewModel(items, response.Result.TotalCount)
        {
            TemplateId = template.Result.Id,
            IsBuiltInTemplate = template.Result.IsBuiltInTemplate,
            ThemeId = themeId,
        };

        return View("~/Areas/Admin/Views/Themes/WebTemplates/Revisions.cshtml", viewModel);
    }

    [HttpPost]
    [Authorize(Policy = BuiltInSystemPermission.MANAGE_TEMPLATES_PERMISSION)]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/web-templates/{{id}}/revisions/{{revisionId}}", Name = "webtemplatesrevisionsrevert")]
    public async Task<IActionResult> RevisionsRevert(string themeId, string id, string revisionId)
    {
        var input = new RevertWebTemplate.Command
        {
            Id = revisionId
        };

        var response = await Mediator.Send(input);

        if (response.Success)
            SetSuccessMessage($"Template has been reverted.");
        else
            SetErrorMessage("There was an error reverting this template", response.GetErrors());

        return RedirectToAction(nameof(WebTemplatesEdit), new { themeId, id, });
    }

    [ServiceFilter(typeof(SetPaginationInformationFilterAttribute))]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/media-items", Name = "mediaitemsindex")]
    public async Task<IActionResult> MediaItemsIndex(string themeId)
    {
        var input = new GetMediaItemsByThemeId.Query
        {
            ThemeId = themeId
        };

        var response = await Mediator.Send(input);
        var items = response.Result.Select(mi => new MediaItemsListItem_ViewModel
        {
            Id = mi.Id,
            FileName = mi.FileName,
            ContentType = mi.ContentType,
            FileStorageProvider = mi.FileStorageProvider,
            ObjectKey = mi.ObjectKey,
        });

        var viewModel = new MediaItemList_ViewModel
        {
            ThemeId = themeId,
            Items = items,
        };

        return View("~/Areas/Admin/Views/Themes/MediaItems/Index.cshtml", viewModel);
    }

    [HttpPost]
    [Route($"{RAYTHA_ROUTE_PREFIX}/themes/edit/{{themeId}}/media-items/delete/{{id}}", Name = "mediaitemsdelete")]
    public async Task<IActionResult> MediaItemsDelete(string themeId, string id)
    {
        var input = new DeleteMediaItem.Command
        {
            Id = id,
        };

        var response = await Mediator.Send(input);

        if (response.Success)
            SetSuccessMessage("Media item has been deleted.");
        else
            SetErrorMessage("There was an error deleting this media item", response.GetErrors());

        return RedirectToAction(nameof(MediaItemsIndex), new { themeId });
    }

    protected List<WebTemplateDto> GetChildren(WebTemplateDto[] list, WebTemplateDto startItem)
    {
        var result = new List<WebTemplateDto>();
        var children = list.Where(p => p.ParentTemplateId == startItem.Id).ToList();
        foreach (var child in children)
        {
            result.Add(child);
            result.AddRange(GetChildren(list, child));
        }
        return result;
    }

    protected Dictionary<string, IEnumerable<WebTemplatesInsertVariableListItem_ViewModel>> GetInsertVariablesViewModel(string templateName, bool isBuiltInTemplate, IEnumerable<ContentTypeDto> contentTypes)
    {
        var templateVariableDictionary = new Dictionary<string, IEnumerable<WebTemplatesInsertVariableListItem_ViewModel>>();
        var requestVariables = InsertVariableTemplateFactory.Request.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
        {
            DeveloperName = p.Key,
            TemplateVariable = p.Value
        });

        var currentOrgVariables = InsertVariableTemplateFactory.CurrentOrganization.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
        {
            DeveloperName = p.Key,
            TemplateVariable = p.Value
        });

        var currentUserVariables = InsertVariableTemplateFactory.CurrentUser.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
        {
            DeveloperName = p.Key,
            TemplateVariable = p.Value
        });

        var navigationMenuVariables = InsertVariableTemplateFactory.NavigationMenu.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel()
        {
            DeveloperName = p.Key,
            TemplateVariable = p.Value,
        });

        var navigationMenuItemVariables = InsertVariableTemplateFactory.NavigationMenuItem.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel()
        {
            DeveloperName = p.Key,
            TemplateVariable = p.Value,
        });

        templateVariableDictionary.Add(InsertVariableTemplateFactory.Request.VariableCategoryName, requestVariables);
        templateVariableDictionary.Add(InsertVariableTemplateFactory.CurrentOrganization.VariableCategoryName, currentOrgVariables);
        templateVariableDictionary.Add(InsertVariableTemplateFactory.CurrentUser.VariableCategoryName, currentUserVariables);
        templateVariableDictionary.Add(InsertVariableTemplateFactory.NavigationMenu.VariableCategoryName, navigationMenuVariables);
        templateVariableDictionary.Add(InsertVariableTemplateFactory.NavigationMenuItem.VariableCategoryName, navigationMenuItemVariables);

        if (ShowContentVariablesForTemplate(templateName) || !isBuiltInTemplate)
        {
            var contentTypeVariables = InsertVariableTemplateFactory.ContentType.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
            {
                DeveloperName = p.Key,
                TemplateVariable = p.Value
            });

            var builtInContentItemVariables = InsertVariableTemplateFactory.ContentItem.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
            {
                DeveloperName = p.Key,
                TemplateVariable = p.Value
            });

            var listResultVariables = InsertVariableTemplateFactory.ContentItemListResult.TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
            {
                DeveloperName = p.Key,
                TemplateVariable = p.Value
            });


            templateVariableDictionary.Add(InsertVariableTemplateFactory.ContentType.VariableCategoryName, contentTypeVariables);
            templateVariableDictionary.Add($"{InsertVariableTemplateFactory.ContentItemListResult.VariableCategoryName} (list result)", listResultVariables);
            templateVariableDictionary.Add($"{InsertVariableTemplateFactory.ContentItem.VariableCategoryName} (single item)", builtInContentItemVariables);

            foreach (var item in contentTypes)
            {
                var allCustomVariables = item.ContentTypeFields.Select(p => new WebTemplatesInsertVariableListItem_ViewModel
                {
                    DeveloperName = $"{p.DeveloperName}{RenderValueProperty(p.FieldType)}",
                    TemplateVariable = $"{InsertVariableTemplateFactory.ContentItem.VariableCategoryName}.PublishedContent.{p.DeveloperName}{RenderValueProperty(p.FieldType)}"
                }).ToList();
                allCustomVariables.AddRange(item.ContentTypeFields.Where(p => p.FieldType.DeveloperName != BaseFieldType.OneToOneRelationship.DeveloperName).Select(p => new WebTemplatesInsertVariableListItem_ViewModel
                {
                    DeveloperName = $"{p.DeveloperName}.Value",
                    TemplateVariable = $"{InsertVariableTemplateFactory.ContentItem.VariableCategoryName}.PublishedContent.{p.DeveloperName}.Value"
                }));
                templateVariableDictionary.Add($"{item.LabelSingular}", allCustomVariables.OrderBy(p => p.DeveloperName));
            }
        }
        else
        {
            var webTemplateVariables = InsertVariableTemplateFactory.From(templateName).TemplateInfo.GetTemplateVariables().Select(p => new WebTemplatesInsertVariableListItem_ViewModel
            {
                DeveloperName = p.Key,
                TemplateVariable = p.Value
            });
            templateVariableDictionary.Add(InsertVariableTemplateFactory.From(templateName).VariableCategoryName, webTemplateVariables);
        }

        return templateVariableDictionary;
    }

    protected string RenderValueProperty(BaseFieldType fieldType)
    {
        return fieldType.DeveloperName != BaseFieldType.OneToOneRelationship.DeveloperName
            ? $".Text"
            : string.Empty;
    }

    protected bool ShowContentVariablesForTemplate(string templateName)
    {
        return BuiltInWebTemplate._Layout.DeveloperName == templateName;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);
        ViewData["ActiveMenu"] = "Themes";
    }
}