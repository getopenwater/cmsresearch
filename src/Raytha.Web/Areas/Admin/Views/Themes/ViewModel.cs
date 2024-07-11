using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Raytha.Web.Areas.Admin.Views.Shared;
using Raytha.Web.Areas.Admin.Views.Shared.ViewModels;

namespace Raytha.Web.Areas.Admin.Views.Themes;

public class ThemesListItem_ViewModel
{
    public string Id { get; init; }
    public string PreviewImageObjectKey { get; init; }

    [Display(Name = "Title")]
    public string Title { get; init; }

    [Display(Name = "Developer Name")]
    public string DeveloperName { get; set; }

    [Display(Name = "Description")]
    public string Description { get; init; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Last modified at:")]
    public string LastModificationTime { get; init; }

    [Display(Name = "Last modified by:")]
    public string LastModifierUser { get; init; }
}

public class ThemesCreate_ViewModel : FormSubmit_ViewModel
{
    [Display(Name = "Title")]
    public string Title { get; init; }

    [Display(Name = "Description")]
    public string Description { get; init; }

    [Display(Name = "Developer Name")]
    public string DeveloperName { get; set; }

    [Display(Name = "Insert default theme media items")]
    public bool InsertDefaultThemeMediaItems { get; set; } = true;

    public string ImageBase64 { get; set; }
    public string ImageFileType { get; set; }
    public string ImageFileName { get; set; }

    [Display(Name = "Image preview")]
    public string ImagePreview { get; set; }

    public bool UseDirectUploadToCloud { get; set; }
    public string PathBase { get; set; }
}

public class ThemesEdit_ViewModel : FormSubmit_ViewModel
{
    public string Id { get; set; }

    [Display(Name = "Title")]
    public string Title { get; init; }

    [Display(Name = "Developer Name")]
    public string DeveloperName { get; set; }

    [Display(Name = "Description")]
    public string Description { get; init; }

    [Display(Name = "Is active")]
    public bool IsActive { get; init; }

    public string PreviewImageObjectKey { get; set; }
    public bool UseDirectUploadToCloud { get; set; }
    public string PathBase { get; set; }
}

public class ThemesActionsMenu_ViewModel
{
    public string ThemeId { get; set; }
}

public class ThemesBackToList_ViewModel
{
    public string ThemeId { get; set; }
    public bool IsWebTemplates { get; set; }
    public bool IsMediaItems { get; set; }
}

public class ThemesRevisionsListItem_ViewModel
{
    public string Id { get; init; }

    [Display(Name = "Created at")]
    public string CreationTime { get; init; }

    [Display(Name = "Created by")]
    public string CreatorUser { get; init; }

    [Display(Name = "Title")]
    public string Title { get; init; }

    [Display(Name = "Description")]
    public string Description { get; init; }

    [Display(Name = "WebTemplates")]
    public string WebTemplates { get; init; }
}

public class ThemeRevisionsPagination_ViewModel : Pagination_ViewModel
{
    public IEnumerable<ThemesRevisionsListItem_ViewModel> Items { get; }
    public string ThemeId { get; set; }

    public ThemeRevisionsPagination_ViewModel(IEnumerable<ThemesRevisionsListItem_ViewModel> items, int totalCount) : base(totalCount)
    {
        Items = items;
    }
}

public class ThemesBeginImportFromUrl_ViewModel : FormSubmit_ViewModel
{
    public string Url { get; set; }
}

public class ThemesBeginExportToUrl_ViewModel : FormSubmit_ViewModel
{
    public string Id { get; set; }

    [Display(Name = "Is can export")]
    public bool IsCanExport { get; set; }

    [Display(Name = "Url")]
    public string Url { get; set; }
}

public class ThemesBackgroundTaskStatus_ViewModel 
{
    public string PathBase { get; set; }
}