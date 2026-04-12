namespace QueryForge.Extensions;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Models;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken ct = default)
    {
        query = query.ApplyFilters(request.Filters);
        query = query.ApplySorting(request.Sorts);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public static async Task<PagedResult<TDestination>> ToPagedResultAsync<T, TDestination>(
        this IQueryable<T> query,
        PagedRequest request,
        IMapper mapper,
        CancellationToken ct = default)
    {
        query = query.ApplyFilters(request.Filters);
        query = query.ApplySorting(request.Sorts);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items1 = mapper.Map<List<T>, List<TDestination>>(items);

        return new PagedResult<TDestination>
        {
            Items = items1,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
