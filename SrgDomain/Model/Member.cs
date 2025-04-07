using System.ComponentModel.DataAnnotations;

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
}
