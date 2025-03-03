using System.ComponentModel.DataAnnotations;
namespace SrgDomain.Model;

public partial class Task : Entity
{
    public int ManagerId { get; set; }
    [Required(ErrorMessage = "Назва обов'язков")]
    [Display(Name = "Назва")]
    public string Title { get; set; } = null!;
    [Required(ErrorMessage = "Опис обов'язковий")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }
    [Display(Name = "Дата створення")]
    public DateTime CreationDate { get; set; }
    [Required(ErrorMessage = "Дедлайн обов'язковий")]
    [Display(Name = "Дедлайн")]
    public DateTime? Deadline { get; set; }
    [Display(Name = "Статус")]
    public string Status { get; set; } = null!;

    public virtual Manager Manager { get; set; } = null!;

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
