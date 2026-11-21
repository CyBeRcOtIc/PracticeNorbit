namespace Timekeeping.Api.Models.Dtos;

public record ProjectDto(Guid Id, string Name, string Code, bool IsActive);

public record CreateProjectRequest(string Name, string Code, bool IsActive);

public record UpdateProjectRequest(string Name, string Code, bool IsActive);