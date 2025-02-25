using System;
using System.Collections.Generic;

namespace SrgDomain.Model;

public partial class Task : Entity
{
    public int ManagerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? Deadline { get; set; }

    public string Status { get; set; } = null!;

    public virtual Manager Manager { get; set; } = null!;

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
