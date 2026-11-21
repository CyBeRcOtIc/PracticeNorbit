namespace Timekeeping.Api.Models.Dtos;

public record TaskDto(Guid Id, string Name, Guid ProjectId, bool IsActive);

public record CreateTaskRequest(string Name, Guid ProjectId, bool IsActive);

public record UpdateTaskRequest(string Name, Guid ProjectId, bool IsActive);
