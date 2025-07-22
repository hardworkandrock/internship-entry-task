using Microsoft.EntityFrameworkCore;
using TestGameWork.Data;
using TestGameWork.Models;

namespace TestGameWork.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;

        public GameRepository(AppDbContext context) => _context = context;

        public async Task<List<Game>> GetAllAsync(Guid playerId)
        {
            return await _context.Games
                .Where(g => g.Status == GameStatus.Active)
                .OrderByDescending(g => g.Player1 == playerId || g.Player2 == playerId)
                .ToListAsync();
        }

        public async Task<List<Game>> GetAllAsync()
        {
            return await _context.Games
            .Where(g => g.Status == GameStatus.Active)
            .ToListAsync();
        }

        public async Task<Game> GetByIdAsync(Guid id) =>
            await _context.Games.FindAsync(id);

        public async Task AddAsync(Game game)
        {
            await _context.Games.AddAsync(game);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Game game)
        { 
            var existingGame = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == game.Id);

            if (existingGame == null)
                throw new InvalidOperationException("Game not found in DB");

            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

    }
}
