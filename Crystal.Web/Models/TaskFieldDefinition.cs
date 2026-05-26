using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.Models;

public class TaskFieldDefinition
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Название поля")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Тип поля")]
    public TaskFieldType FieldType { get; set; }

    [Display(Name = "Обязательное")]
    public bool IsRequired { get; set; }

    [Display(Name = "Базовое")]
    public bool IsBase { get; set; }

    public ICollection<TaskFieldValue> Values { get; set; } = new List<TaskFieldValue>();
}
