using Crystal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

// модель списка задач, содержит сами задачи фильтры и данные для пагинации
public class TaskIndexViewModel
{
    public IEnumerable<TaskItem> Tasks { get; set; } = [];
    public string? Search { get; set; }
    // выбранный фильтр статуса
    public int? StatusId { get; set; }
    // выбранный фильтр приоритета
    public TaskPriority? Priority { get; set; }
    // выбранный фильтр проекта
    public int? ProjectId { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    // статусы для фильтра
    public List<SelectListItem> Statuses { get; set; } = [];
    // проекты для фильтра
    public List<SelectListItem> Projects { get; set; } = [];
}
