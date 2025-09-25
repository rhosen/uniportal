using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;

public class NoticeService
{
    private readonly UniPortalContext _context;

    public NoticeService(UniPortalContext context)
    {
        _context = context;
    }

    public async Task<List<Notice>> GetAllAsync()
    {
        return await _context.Notices
            .Where(x => !x.IsDeleted)
            .Include(n => n.RecipientType)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notice> GetByIdAsync(string id)
    {
        return await _context.Notices
            .Include(n => n.RecipientType)
            .FirstOrDefaultAsync(n => n.Id.ToString() == id);
    }

    public async Task CreateAsync(string title, string message, Guid createdBy, Guid recipientTypeId, string? recipientId, string? filePath = null)
    {
        var recipientEnum = await GetRecipientEnumAsync(recipientTypeId);

        await ValidateRecipientAsync(recipientEnum, recipientId);

        var notification = new Notice
        {
            Title = title,
            Message = message,
            ModifiedById = createdBy,
            RecipientTypeId = recipientTypeId,
            RecipientId = recipientId,
            FilePath = filePath
        };

        _context.Notices.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, string title, string message, Guid recipientTypeId, string? recipientId, string? filePath = null)
    {
        var notification = await _context.Notices.FindAsync(id);
        if (notification == null)
            throw new InvalidOperationException("Notice not found.");

        var recipientEnum = await GetRecipientEnumAsync(recipientTypeId);

        await ValidateRecipientAsync(recipientEnum, recipientId);

        notification.Title = title;
        notification.Message = message;
        notification.RecipientTypeId = recipientTypeId;
        notification.RecipientId = recipientId;
        notification.FilePath = filePath;
        notification.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    private async Task<Recipient> GetRecipientEnumAsync(Guid recipientTypeId)
    {
        var recipientType = await _context.RecipientTypes
            .FirstOrDefaultAsync(r => r.Id == recipientTypeId && !r.IsDeleted);

        if (recipientType == null)
            throw new InvalidOperationException("Invalid recipient type.");

        return recipientType.Name.ToLower() switch
        {
            "all" => Recipient.All,
            "faculty" => Recipient.Faculty,
            "student" => Recipient.Student,
            _ => throw new InvalidOperationException("Unsupported recipient type.")
        };
    }

    private async Task ValidateRecipientAsync(Recipient type, string? recipientId)
    {
        switch (type)
        {
            case Recipient.All:
                // No recipientId required
                break;

            case Recipient.Faculty:
                if (string.IsNullOrEmpty(recipientId) || !await _context.Faculties.AnyAsync(f => f.FacultyNumber == recipientId))
                    throw new InvalidOperationException("Faculty number does not exist.");
                break;

            case Recipient.Student:
                if (string.IsNullOrEmpty(recipientId) || !await _context.Students.AnyAsync(s => s.StudentNumber == recipientId))
                    throw new InvalidOperationException("Student number does not exist.");
                break;

            default:
                throw new InvalidOperationException("Unsupported recipient type.");
        }
    }

    public async Task DeleteAsync(string id)
    {
        var notification = await _context.Notices.FindAsync(Guid.Parse(id));
        if (notification != null)
        {
            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ActivateAsync(string id)
    {
        var notification = await _context.Notices.FindAsync(Guid.Parse(id));
        if (notification != null)
        {
            notification.IsDeleted = false;
            notification.DeletedAt = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<RecipientType>> GetNotificationTypesAsync()
    {
        return await _context.RecipientTypes
            .Where(nt => !nt.IsDeleted)
            .OrderBy(nt => nt.Name)
            .ToListAsync();
    }
}
