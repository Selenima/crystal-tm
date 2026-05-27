using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

// модель формы очереди, используется для создания и редактирования
public class QueueEditViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Название очереди")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Пользователи")]
    public List<int> SelectedUserIds { get; set; } = [];

    public List<SelectListItem> AllUsers { get; set; } = [];
}
