using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Dtos;

namespace Timekeeping.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app.MapGroup("/api/tasks").WithTags("Tasks");

        tasks.MapGet("/", async (Guid? projectId, AppDbContext db) =>
        {
            var query = db.Tasks.AsNoTracking().AsQueryable();

            if (projectId is not null)
                query = query.Where(t => t.ProjectId == projectId);

            return await query
                .Select(t => new TaskDto(t.Id, t.Name, t.ProjectId, t.IsActive))
                .ToListAsync();
        });

        tasks.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
            await db.Tasks.FindAsync(id) is TaskItem task
                ? Results.Ok(new TaskDto(task.Id, task.Name, task.ProjectId, task.IsActive))
                : Results.NotFound());

        tasks.MapPost("/", async (CreateTaskRequest input, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return Results.BadRequest("Name is required.");

            var project = await db.Projects.FindAsync(input.ProjectId);
            if (project is null)
                return Results.BadRequest("ProjectId does not reference an existing project.");

            var task = new TaskItem
            {
                Name = input.Name,
                ProjectId = input.ProjectId,
                IsActive = input.IsActive
            };

            db.Tasks.Add(task);
            await db.SaveChangesAsync();

            var dto = new TaskDto(task.Id, task.Name, task.ProjectId, task.IsActive);
            return Results.Created($"/api/tasks/{task.Id}", dto);
        });

        tasks.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest input, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(input.Name))
                return Results.BadRequest("Name is required.");

            var project = await db.Projects.FindAsync(input.ProjectId);
            if (project is null)
                return Results.BadRequest("ProjectId does not reference an existing project.");

            task.Name = input.Name;
            task.ProjectId = input.ProjectId;
            task.IsActive = input.IsActive;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        tasks.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound();

            db.Tasks.Remove(task);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Cannot delete a task that still has time entries.");
            }

            return Results.NoContent();
        });
    }
}
