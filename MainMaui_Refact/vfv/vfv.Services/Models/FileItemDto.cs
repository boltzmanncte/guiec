namespace vfv.Services.Models;

/// <summary>
/// Data Transfer Object for FileItem - used for persistence without UI dependencies
/// </summary>
public class FileItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Modified { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}