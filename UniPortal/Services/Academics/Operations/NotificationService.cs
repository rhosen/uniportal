using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Dtos;

public class NotificationService
{
    private readonly UniPortalContext _context;

    public NotificationService(UniPortalContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Return DTOs (includes RecipientNumber and RecipientTypeName).
    /// This fetches notifications, then resolves recipient numbers in bulk (no N+1 queries).
    /// </summary>
    public async Task<List<NotificationDto>> GetAllAsync()
    {
        var notifications = await _context.Notifications
            .Where(x => !x.IsDeleted)
            .Include(n => n.RecipientType)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        // Collect accountIds to resolve recipient numbers in bulk
        var accountIds = notifications
            .Where(n => n.AccountId.HasValue)
            .Select(n => n.AccountId!.Value)
            .Distinct()
            .ToList();

        var facultyMap = new Dictionary<Guid, string>();
        var studentMap = new Dictionary<Guid, string>();

        if (accountIds.Any())
        {
            facultyMap = await _context.Faculties
                .Where(f => accountIds.Contains(f.AccountId))
                .ToDictionaryAsync(f => f.AccountId, f => f.FacultyNumber);

            studentMap = await _context.Students
                .Where(s => accountIds.Contains(s.AccountId))
                .ToDictionaryAsync(s => s.AccountId, s => s.StudentNumber);
        }

        var dtos = notifications.Select(n =>
        {
            string? recipientNumber = null;
            if (n.AccountId.HasValue && n.RecipientType != null)
            {
                var aid = n.AccountId.Value;
                var typeName = n.RecipientType.Name?.ToLowerInvariant();
                if (typeName == "faculty")
                {
                    facultyMap.TryGetValue(aid, out recipientNumber);
                }
                else if (typeName == "student")
                {
                    studentMap.TryGetValue(aid, out recipientNumber);
                }
            }

            return new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                RecipientTypeId = n.RecipientTypeId,
                RecipientTypeName = n.RecipientType?.Name,
                RecipientNumber = recipientNumber,
                AccountId = n.AccountId,
                FilePath = n.FilePath,
                CreatedAt = n.CreatedAt,
                IsDeleted = n.IsDeleted
            };
        }).ToList();

        return dtos;
    }

    /// <summary>
    /// Return single DTO for edit/view (includes resolved RecipientNumber).
    /// </summary>
    public async Task<NotificationDto?> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            return null;

        var n = await _context.Notifications
            .Include(x => x.RecipientType)
            .FirstOrDefaultAsync(x => x.Id == guidId);

        if (n == null)
            return null;

        var recipientNumber = await ResolveRecipientNumberAsync(n.AccountId, n.RecipientTypeId);

        return new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            RecipientTypeId = n.RecipientTypeId,
            RecipientTypeName = n.RecipientType?.Name,
            RecipientNumber = recipientNumber,
            AccountId = n.AccountId,
            FilePath = n.FilePath,
            CreatedAt = n.CreatedAt,
            IsDeleted = n.IsDeleted
        };
    }

    /// <summary>
    /// Create: resolves RecipientNumber -> AccountId and saves Notification entity.
    /// </summary>
    public async Task CreateAsync(NotificationDto dto, Guid createdBy)
    {
        var recipientEnum = await GetRecipientEnumAsync(dto.RecipientTypeId);
        Guid? accountId = null;

        if (recipientEnum != Recipient.All)
        {
            accountId = await GetAccountIdByRecipientNumberAsync(recipientEnum, dto.RecipientNumber);
        }

        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            ModifiedById = createdBy,
            RecipientTypeId = dto.RecipientTypeId,
            AccountId = accountId,
            FilePath = dto.FilePath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Update: resolves RecipientNumber -> AccountId and updates entity.
    /// </summary>
    public async Task UpdateAsync(NotificationDto dto)
    {
        var notification = await _context.Notifications.FindAsync(dto.Id);
        if (notification == null)
            throw new InvalidOperationException("Notification not found.");

        var recipientEnum = await GetRecipientEnumAsync(dto.RecipientTypeId);
        Guid? accountId = null;

        if (recipientEnum != Recipient.All)
        {
            accountId = await GetAccountIdByRecipientNumberAsync(recipientEnum, dto.RecipientNumber);
        }

        notification.Title = dto.Title;
        notification.Message = dto.Message;
        notification.RecipientTypeId = dto.RecipientTypeId;
        notification.AccountId = accountId;
        notification.FilePath = dto.FilePath;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft-delete
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return;

        var notification = await _context.Notifications.FindAsync(guid);
        if (notification != null)
        {
            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Un-delete
    /// </summary>
    public async Task ActivateAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return;

        var notification = await _context.Notifications.FindAsync(guid);
        if (notification != null)
        {
            notification.IsDeleted = false;
            notification.DeletedAt = null;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Recipient types for dropdown (entity kept as before).
    /// </summary>
    public async Task<List<RecipientType>> GetNotificationTypesAsync()
    {
        return await _context.RecipientTypes
            .Where(nt => !nt.IsDeleted)
            .OrderBy(nt => nt.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Map RecipientType row -> Recipient enum
    /// </summary>
    public async Task<Recipient> GetRecipientEnumAsync(Guid recipientTypeId)
    {
        var recipientType = await _context.RecipientTypes
            .FirstOrDefaultAsync(r => r.Id == recipientTypeId && !r.IsDeleted);

        if (recipientType == null)
            throw new InvalidOperationException("Invalid recipient type.");

        return recipientType.Name.ToLowerInvariant() switch
        {
            "all" => Recipient.All,
            "faculty" => Recipient.Faculty,
            "student" => Recipient.Student,
            _ => throw new InvalidOperationException("Unsupported recipient type.")
        };
    }

    /// <summary>
    /// Resolve AccountId from human number (throws if not found).
    /// </summary>
    private async Task<Guid?> GetAccountIdByRecipientNumberAsync(Recipient type, string? recipientNumber)
    {
        if (string.IsNullOrWhiteSpace(recipientNumber))
            throw new InvalidOperationException("Recipient number is required.");

        switch (type)
        {
            case Recipient.Faculty:
                {
                    var facultyAccount = await _context.Faculties
                        .Where(f => f.FacultyNumber == recipientNumber)
                        .Select(f => f.AccountId)
                        .FirstOrDefaultAsync();

                    if (facultyAccount == Guid.Empty)
                        throw new InvalidOperationException("Faculty number does not exist.");

                    return facultyAccount;
                }

            case Recipient.Student:
                {
                    var studentAccount = await _context.Students
                        .Where(s => s.StudentNumber == recipientNumber)
                        .Select(s => s.AccountId)
                        .FirstOrDefaultAsync();

                    if (studentAccount == Guid.Empty)
                        throw new InvalidOperationException("Student number does not exist.");

                    return studentAccount;
                }

            default:
                return null;
        }
    }

    /// <summary>
    /// Reverse lookup: given AccountId and recipient type, return the human number.
    /// </summary>
    public async Task<string?> ResolveRecipientNumberAsync(Guid? accountId, Guid recipientTypeId)
    {
        if (accountId == null) return null;

        var recipientEnum = await GetRecipientEnumAsync(recipientTypeId);

        return recipientEnum switch
        {
            Recipient.Faculty => await _context.Faculties
                .Where(f => f.AccountId == accountId)
                .Select(f => f.FacultyNumber)
                .FirstOrDefaultAsync(),

            Recipient.Student => await _context.Students
                .Where(s => s.AccountId == accountId)
                .Select(s => s.StudentNumber)
                .FirstOrDefaultAsync(),

            _ => null
        };
    }
}
