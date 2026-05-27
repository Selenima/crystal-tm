using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Crystal.Web.ViewModels;

// модель формы перехода статуса, она связывает from и to статус
public class StatusTransitionEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Из статуса")]
    public int FromTaskStatusEntityId { get; set; }

    [Display(Name = "В статус")]
    public int ToTaskStatusEntityId { get; set; }

    public List<SelectListItem> Statuses { get; set; } = [];
}
