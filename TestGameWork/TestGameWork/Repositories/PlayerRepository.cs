using System.Reflection.Metadata.Ecma335;
using TestGameWork.Data;
using TestGameWork.Models;

namespace TestGameWork.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AppDbContext _context; 
        public PlayerRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Player player) =>
            await _context.Players.AddAsync(player);

        public async Task<Player> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.Players.FindAsync(id);

        public Player GetById(Guid id)
        {
            var player = _context.Players.Find(id);
            if (player != null) return player;

            return new Player
            {
                Id = Guid.Empty,
                Name = "",
                HashPassword = ""
            };
        }
    }
}
