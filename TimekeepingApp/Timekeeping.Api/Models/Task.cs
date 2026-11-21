using System;
using System.Collections.Generic;

namespace Timekeeping.Api.Models;

public partial class TaskItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid ProjectId { get; set; }

    public bool IsActive { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
