using Crystal.Web.Models;

namespace Crystal.Web.ViewModels;

public class TaskFieldInputViewModel
{
    public int DefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TaskFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsBase { get; set; }
    public string? Value { get; set; }
}
