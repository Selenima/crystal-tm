using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

// конкретное значение дополнительного поля у конкретной задачи
public class TaskFieldValue
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int TaskFieldDefinitionId { get; set; }

    [StringLength(500)]
    [Display(Name = "Значение")]
    public string? Value { get; set; }

    public TaskItem? TaskItem { get; set; }
    public TaskFieldDefinition? TaskFieldDefinition { get; set; }
}
