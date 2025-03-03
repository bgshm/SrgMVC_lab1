using System.ComponentModel.DataAnnotations;

namespace SrgDomain.Model;

public partial class Department : Entity
{
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [Display(Name = "Назва департаменту")]
    public string DepartmentName { get; set; } = null!;
    [Required(ErrorMessage = "Опис обов'язковий")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Пошта обов'язкова")]
    [Display(Name = "Контактна пошта")]
    public string? ContactEmail { get; set; }
    [Display(Name = "Дата створення")]
    public DateTime CreatedDate { get; set; }
    public virtual Manager? Manager { get; set; }
}
