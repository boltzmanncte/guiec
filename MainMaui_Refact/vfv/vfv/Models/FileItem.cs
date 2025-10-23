using CommunityToolkit.Mvvm.ComponentModel;

namespace vfv.Models;

public partial class FileItem : ObservableObject
{
    [ObservableProperty]
    private string id = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string extension = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string size = string.Empty;

    [ObservableProperty]
    private string modified = string.Empty;

    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isActive;

    public Color IconColor => Extension.ToLower() switch
    {
        "xml" => Color.FromArgb("#FF6600"),
        "json" => Color.FromArgb("#0066CC"),
        _ => Colors.Gray
    };
}
