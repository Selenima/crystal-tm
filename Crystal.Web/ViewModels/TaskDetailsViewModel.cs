using Crystal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

// модель страницы деталей задачи, тут задача и доступные переходы статуса
public class TaskDetailsViewModel
{
    public TaskItem Task { get; set; } = new();
    public List<SelectListItem> AllowedTransitions { get; set; } = [];
}
