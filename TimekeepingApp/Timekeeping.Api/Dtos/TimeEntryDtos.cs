namespace Timekeeping.Api.Models.Dtos;

public record TimeEntryDto(Guid Id, Guid TaskId, DateOnly Date, decimal Hours, string Description);

public record CreateTimeEntryRequest(Guid TaskId, DateOnly Date, decimal Hours, string Description);

public record UpdateTimeEntryRequest(Guid TaskId, DateOnly Date, decimal Hours, string Description);