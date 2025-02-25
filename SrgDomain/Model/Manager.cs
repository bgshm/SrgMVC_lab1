using System;
using System.Collections.Generic;

namespace SrgDomain.Model;

public partial class Manager : Entity
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
