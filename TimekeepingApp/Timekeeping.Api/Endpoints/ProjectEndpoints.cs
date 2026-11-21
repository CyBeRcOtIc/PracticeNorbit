using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Dtos;

namespace Timekeeping.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("/api/projects").WithTags("Projects");

        projects.MapGet("/", async (AppDbContext db) =>
            await db.Projects
                .AsNoTracking()
                .Select(p => new ProjectDto(p.Id, p.Name, p.Code, p.IsActive))
                .ToListAsync());

        projects.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
            await db.Projects.FindAsync(id) is Project project
                ? Results.Ok(new ProjectDto(project.Id, project.Name, project.Code, project.IsActive))
                : Results.NotFound());

        projects.MapPost("/", async (CreateProjectRequest input, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Code))
                return Results.BadRequest("Name and Code are required.");

            var project = new Project
            {
                Name = input.Name,
                Code = input.Code,
                IsActive = input.IsActive
            };

            db.Projects.Add(project);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Project with this code already exists.");
            }

            var dto = new ProjectDto(project.Id, project.Name, project.Code, project.IsActive);
            return Results.Created($"/api/projects/{project.Id}", dto);
        });

        projects.MapPut("/{id:guid}", async (Guid id, UpdateProjectRequest input, AppDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);
            if (project is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Code))
                return Results.BadRequest("Name and Code are required.");

            project.Name = input.Name;
            project.Code = input.Code;
            project.IsActive = input.IsActive;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        projects.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);
            if (project is null) return Results.NotFound();

            db.Projects.Remove(project);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Cannot delete a project that still has tasks.");
            }

            return Results.NoContent();
        });
    }
}