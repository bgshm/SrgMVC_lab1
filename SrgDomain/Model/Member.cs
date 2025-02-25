using System;
using System.Collections.Generic;

namespace SrgDomain.Model;

public partial class Member : Entity
{
    public int ManagerId { get; set; }

    public string Name { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? TasksPerMonth { get; set; }

    public int? TasksTotal { get; set; }

    public DateTime? LastTaskDate { get; set; }

    public virtual Manager Manager { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
