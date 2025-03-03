using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SrgDomain.Model;

public partial class TaskHistory : Entity
{
    public int TaskId { get; set; }
    [Display(Name = "Дія")]
    public string Action { get; set; } = null!;
    [Display(Name = "Час")]
    public DateTime Timestamp { get; set; }

    public virtual Task Task { get; set; } = null!;
}
