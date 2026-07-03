namespace backend.DTOs.Common;

public record PaginatedResponseDTO<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);