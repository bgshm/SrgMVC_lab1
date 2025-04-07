using System.ComponentModel.DataAnnotations;
namespace SrgDomain.Model;

public partial class Manager : Entity
{
    public int DepartmentId { get; set; }
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [Display(Name = "Ім'я")]
    public string Name { get; set; } = null!;

    [Display(Name = "Фото менеджера")]
    public string? PhotoPath { get; set; }
    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
