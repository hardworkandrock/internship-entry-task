using TestGameWork.Models;

namespace TestGameWork.Repositories
{
    public interface IGameRepository
    {
        Task<List<Game>> GetAllAsync();
        Task<List<Game>> GetAllAsync(Guid playerId);
        Task<Game> GetByIdAsync(Guid gameId);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
    }
}
