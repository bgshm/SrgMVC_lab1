using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SrgDomain.Model;

public partial class Member : Entity
{
    public int ManagerId { get; set; }
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [Display(Name = "Ім'я")]
    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "Роль обов'язкова")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = null!;
    [Required(ErrorMessage = "Структурний підрозділ обов'язковий")]
    [Display(Name = "Структурний підрозділ")]
    public string StructuralUnit { get; set; } = "";
    [Required(ErrorMessage = "Рік вступу обов'язковий")]
    [Display(Name = "Рік вступу")]
    public int EnrollmentYear { get; set; }
    [Display(Name = "Виконані завдання за місяць")]
    public int? TasksPerMonth { get; set; }
    [Display(Name = "Виконані завдання за весь час")]
    public int? TasksTotal { get; set; }
    [Display(Name = "Дата останнього виконаного завдання")]
    public DateTime? LastTaskDate { get; set; }

    [Display(Name = "Фото учасника")]
    public string? PhotoPath { get; set; }

    public virtual Manager Manager { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    [NotMapped]
    public int Course
    {
        get
        {
            var now = DateTime.Now;
            int baseYear = (now.Month < 7 ? now.Year - 1 : now.Year);
            return (baseYear - EnrollmentYear) + 1;
        }
    }
}
