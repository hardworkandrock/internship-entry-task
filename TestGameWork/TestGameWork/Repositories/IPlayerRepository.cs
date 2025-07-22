using TestGameWork.Models;

namespace TestGameWork.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player> GetByIdAsync(Guid id, CancellationToken ct = default);
        Player GetById(Guid id);
        Task AddAsync(Player game); 
    }
}
