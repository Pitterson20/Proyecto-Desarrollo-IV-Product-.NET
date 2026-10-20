namespace Ferreteria.DTOs.Common;

public sealed class PaginatedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];

    public PaginationLinks Links { get; init; } = new();

    public PaginationMeta Meta { get; init; } = new();
}

public sealed class PaginationLinks
{
    public string First { get; init; } = string.Empty;

    public string Last { get; init; } = string.Empty;

    public string? Prev { get; init; }

    public string? Next { get; init; }
}

public sealed class PaginationMeta
{
    public int CurrentPage { get; init; }

    public int? From { get; init; }

    public int LastPage { get; init; }

    public string Path { get; init; } = string.Empty;

    public int PerPage { get; init; }

    public int? To { get; init; }

    public int Total { get; init; }
}
