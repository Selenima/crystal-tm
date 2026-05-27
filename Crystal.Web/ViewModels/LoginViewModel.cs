using System.ComponentModel.DataAnnotations;

namespace Crystal.Web.ViewModels;

// данные формы входа, пользователь вводит email и пароль
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}
