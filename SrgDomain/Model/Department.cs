using System;
using System.Collections.Generic;

namespace SrgDomain.Model;

public partial class Department : Entity
{
    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public string? ContactEmail { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Manager? Manager { get; set; }
}
