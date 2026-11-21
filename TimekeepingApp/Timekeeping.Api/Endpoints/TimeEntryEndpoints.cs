// Endpoints/TimeEntryEndpoints.cs
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Dtos;

namespace Timekeeping.Api.Endpoints;

public static class TimeEntryEndpoints
{
    public static void MapTimeEntryEndpoints(this IEndpointRouteBuilder app)
    {
        var timeEntries = app.MapGroup("/api/time-entries").WithTags("TimeEntries");

        // filters: ?date=2026-11-21  OR  ?year=2026&month=11  OR nothing (= all time)
        timeEntries.MapGet("/", async (DateOnly? date, int? year, int? month, AppDbContext db) =>
        {
            var query = db.TimeEntries.AsNoTracking().AsQueryable();

            if (date is not null)
            {
                query = query.Where(e => e.Date == date);
            }
            else if (year is not null && month is not null)
            {
                query = query.Where(e => e.Date.Year == year && e.Date.Month == month);
            }

            return await query
                .OrderByDescending(e => e.Date)
                .Select(e => new TimeEntryDto(e.Id, e.TaskId, e.Date, e.Hours, e.Description))
                .ToListAsync();
        });

        // sticker: total hours for a day + status (Under / Exact / Over)
        timeEntries.MapGet("/daily-summary", async (DateOnly date, AppDbContext db) =>
        {
            var totalHours = await db.TimeEntries
                .Where(e => e.Date == date)
                .SumAsync(e => (decimal?)e.Hours) ?? 0m;

            var status = totalHours < 8 ? "Under" : totalHours == 8 ? "Exact" : "Over";

            return Results.Ok(new { date, totalHours, status });
        });

        timeEntries.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
            await db.TimeEntries.FindAsync(id) is TimeEntry entry
                ? Results.Ok(new TimeEntryDto(entry.Id, entry.TaskId, entry.Date, entry.Hours, entry.Description))
                : Results.NotFound());

        timeEntries.MapPost("/", async (CreateTimeEntryRequest input, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(input.Description))
                return Results.BadRequest("Description is required.");

            if (input.Hours <= 0 || input.Hours > 24)
                return Results.BadRequest("Hours must be greater than 0 and at most 24.");

            var task = await db.Tasks.FindAsync(input.TaskId);
            if (task is null)
                return Results.BadRequest("TaskId does not reference an existing task.");

            if (!task.IsActive)
                return Results.BadRequest("Cannot log time against an inactive task.");

            var hoursAlready = await db.TimeEntries
                .Where(e => e.Date == input.Date)
                .SumAsync(e => (decimal?)e.Hours) ?? 0m;

            if (hoursAlready + input.Hours > 24)
                return Results.BadRequest($"Total hours for {input.Date} would exceed 24 (already logged: {hoursAlready}).");

            var entry = new TimeEntry
            {
                TaskId = input.TaskId,
                Date = input.Date,
                Hours = input.Hours,
                Description = input.Description
            };

            db.TimeEntries.Add(entry);
            await db.SaveChangesAsync();

            var dto = new TimeEntryDto(entry.Id, entry.TaskId, entry.Date, entry.Hours, entry.Description);
            return Results.Created($"/api/time-entries/{entry.Id}", dto);
        });

        timeEntries.MapPut("/{id:guid}", async (Guid id, UpdateTimeEntryRequest input, AppDbContext db) =>
        {
            var entry = await db.TimeEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(input.Description))
                return Results.BadRequest("Description is required.");

            if (input.Hours <= 0 || input.Hours > 24)
                return Results.BadRequest("Hours must be greater than 0 and at most 24.");

            // rule: task field is frozen once its task became inactive
            if (input.TaskId != entry.TaskId)
            {
                var currentTask = await db.Tasks.FindAsync(entry.TaskId);
                if (currentTask is null || !currentTask.IsActive)
                    return Results.Conflict("Cannot change the task: the current task is inactive.");

                var newTask = await db.Tasks.FindAsync(input.TaskId);
                if (newTask is null)
                    return Results.BadRequest("TaskId does not reference an existing task.");
                if (!newTask.IsActive)
                    return Results.BadRequest("Cannot log time against an inactive task.");

                entry.TaskId = input.TaskId;
            }

            var hoursOtherEntries = await db.TimeEntries
                .Where(e => e.Date == input.Date && e.Id != id)
                .SumAsync(e => (decimal?)e.Hours) ?? 0m;

            if (hoursOtherEntries + input.Hours > 24)
                return Results.BadRequest($"Total hours for {input.Date} would exceed 24 (already logged: {hoursOtherEntries}).");

            entry.Date = input.Date;
            entry.Hours = input.Hours;
            entry.Description = input.Description;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        timeEntries.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var entry = await db.TimeEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();

            db.TimeEntries.Remove(entry);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}