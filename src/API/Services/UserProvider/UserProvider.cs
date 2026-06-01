using BLL.Common.Interfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Services.UserProvider;

public class UserProvider(IHttpContextAccessor context, AppDbContext appDbContext) : IUserProvider
{
    private readonly IHttpContextAccessor _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Guid> GetUserId(CancellationToken cancellationToken = default)
    {
        if (_context.HttpContext == null) return Guid.Empty;

        var userIdStr = _context.HttpContext.User.FindFirst("id")?.Value;

        if (userIdStr == null)
        {
            return Guid.Empty; // SignalR or unauthenticated
        }

        var userIdGuid = Guid.Parse(userIdStr);

        if (await appDbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userIdGuid, cancellationToken) == null)
        {
            throw new InvalidOperationException("User does not exist.");
        }

        return userIdGuid;
    }

    public string GetUserRole()
    {
        var userRole = _context.HttpContext!.User.Claims
            .FirstOrDefault(c => c.Type.Contains("role", StringComparison.OrdinalIgnoreCase))?.Value;
        
        if (userRole == null)
        {
            throw new InvalidOperationException("User role claim not found.");
        }
        
        return userRole;
    } 
}