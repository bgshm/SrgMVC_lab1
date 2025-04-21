using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SrgDomain.Model;

public partial class Manager : Entity
{
    public int DepartmentId { get; set; }
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [Display(Name = "Ім'я")]
    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "Структурний підрозділ обов'язковий")]
    [Display(Name = "Структурний підрозділ")]
    public string StructuralUnit { get; set; } = "";
    [Required(ErrorMessage = "Рік вступу обов'язковий")]
    [Display(Name = "Рік вступу")]
    public int EnrollmentYear { get; set; }

    [Display(Name = "Фото менеджера")]
    public string? PhotoPath { get; set; }
    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    [NotMapped]
    public int Course
    {
        get
        {
            var now = DateTime.Now;
            // academic year rolls over on July 1:
            int baseYear = (now.Month < 7 ? now.Year - 1 : now.Year);
            return (baseYear - EnrollmentYear) + 1;
        }
    }
}
