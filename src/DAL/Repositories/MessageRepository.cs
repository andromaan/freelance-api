using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Messages;
using DAL.Data;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class MessageRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<Message, Guid>(appDbContext, userProvider), IMessageRepository, IMessageQueries
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IUserProvider _userProvider = userProvider;

    public Task<List<Message>> GetByUserAsync(CancellationToken cancellationToken)
    {
        var userId = _userProvider.GetUserId().GetAwaiter().GetResult();

        return _appDbContext.Set<Message>().Where(m => m.CreatedBy == userId || m.ReceiverId == userId).OrderBy(m => m.SentAt)
            .AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<List<Message>> GetByContractAsync(Guid contractId, CancellationToken cancellationToken)
    {
        var userId = _userProvider.GetUserId().GetAwaiter().GetResult();

        return _appDbContext.Set<Message>()
            .Where(m => (m.CreatedBy == userId || m.ReceiverId == userId) 
                && m.ContractId == contractId)
            .OrderBy(m => m.SentAt)
            .AsNoTracking().ToListAsync(cancellationToken);
    }
}