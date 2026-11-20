using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class ReadingListService
{
    private readonly LibraryDbContext _context;

    public ReadingListService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ReadingListResponse>> GetReadingListsAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.ReadingLists
            .Include(rl => rl.Items).ThenInclude(i => i.Book)
            .Where(rl => rl.UserId == userId)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var entities = await query
            .OrderByDescending(rl => rl.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapToResponse).ToList();
        return new PagedResult<ReadingListResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<ReadingListResponse?> GetReadingListAsync(int id, string userId, CancellationToken ct = default)
    {
        var list = await _context.ReadingLists
            .Include(rl => rl.Items).ThenInclude(i => i.Book)
            .AsNoTracking()
            .FirstOrDefaultAsync(rl => rl.Id == id && rl.UserId == userId, ct);
        return list is null ? null : MapToResponse(list);
    }

    public async Task<ReadingListResponse> CreateReadingListAsync(string userId, ReadingListRequest request, CancellationToken ct = default)
    {
        var list = new ReadingList
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Items = new List<ReadingListItem>()
        };

        _context.ReadingLists.Add(list);
        await _context.SaveChangesAsync(ct);

        return await GetReadingListAsync(list.Id, userId, ct)
            ?? throw new InvalidOperationException("Failed to retrieve the created reading list.");
    }

    public async Task<bool> UpdateReadingListAsync(int id, string userId, ReadingListRequest request, CancellationToken ct = default)
    {
        var list = await _context.ReadingLists.FirstOrDefaultAsync(rl => rl.Id == id && rl.UserId == userId, ct);
        if (list is null) return false;
        list.Name = request.Name;
        list.Description = request.Description;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteReadingListAsync(int id, string userId, CancellationToken ct = default)
    {
        var list = await _context.ReadingLists.FirstOrDefaultAsync(rl => rl.Id == id && rl.UserId == userId, ct);
        if (list is null) return false;
        _context.ReadingLists.Remove(list);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ReadingListResponse> AddItemAsync(int listId, string userId, AddReadingListItemRequest request, CancellationToken ct = default)
    {
        var list = await _context.ReadingLists
            .Include(rl => rl.Items)
            .FirstOrDefaultAsync(rl => rl.Id == listId && rl.UserId == userId, ct)
            ?? throw new InvalidOperationException($"Reading list {listId} not found or access denied.");

        var bookExists = await _context.Books.AnyAsync(b => b.Id == request.BookId, ct);
        if (!bookExists)
            throw new InvalidOperationException($"Book {request.BookId} not found.");

        var alreadyAdded = list.Items.Any(i => i.BookId == request.BookId);
        if (alreadyAdded)
            throw new InvalidOperationException($"Book {request.BookId} is already in reading list {listId}.");

        var item = new ReadingListItem { ReadingListId = listId, BookId = request.BookId };
        _context.ReadingListItems.Add(item);
        await _context.SaveChangesAsync(ct);

        return await GetReadingListAsync(listId, userId, ct)
            ?? throw new InvalidOperationException("Failed to retrieve the updated reading list.");
    }

    public async Task<bool> RemoveItemAsync(int listId, int itemId, string userId, CancellationToken ct = default)
    {
        var item = await _context.ReadingListItems
            .Include(i => i.ReadingList)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ReadingList!.UserId == userId, ct);
        if (item is null) return false;
        _context.ReadingListItems.Remove(item);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ItemExistsAsync(int listId, int bookId, CancellationToken ct = default)
    {
        return await _context.ReadingListItems
            .AnyAsync(i => i.ReadingListId == listId && i.BookId == bookId, ct);
    }

    private static ReadingListResponse MapToResponse(ReadingList rl)
    {
        return new ReadingListResponse
        {
            Id = rl.Id,
            UserId = rl.UserId,
            Name = rl.Name,
            Description = rl.Description,
            ItemCount = rl.Items.Count,
            Items = rl.Items.Select(i => new ReadingListItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book?.Title ?? ""
            }).ToList()
        };
    }
}
