using System;
using System.Collections.Generic;

namespace SrgDomain.Model;

public partial class TaskHistory : Entity
{
    public int TaskId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual Task Task { get; set; } = null!;
}
