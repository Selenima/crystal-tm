using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

// модель формы пользователя в админке
public class UserEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Должность")]
    public string Position { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Отдел")]
    public string Department { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Display(Name = "Администратор")]
    public bool IsAdmin { get; set; }

    [Display(Name = "Очередь")]
    public int? WorkQueueId { get; set; }

    // очереди для select
    public List<SelectListItem> Queues { get; set; } = [];
    [Display(Name = "Проекты")]
    public List<int> SelectedProjectIds { get; set; } = [];
    public List<SelectListItem> Projects { get; set; } = [];
}
