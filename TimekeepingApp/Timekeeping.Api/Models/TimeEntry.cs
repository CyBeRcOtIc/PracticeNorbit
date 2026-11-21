using System;
using System.Collections.Generic;

namespace Timekeeping.Api.Models;

public partial class TimeEntry
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string Description { get; set; } = null!;

    public virtual TaskItem Task { get; set; } = null!;
}
