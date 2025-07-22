using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGameWork.Models;
using TestGameWork.Repositories;

namespace TestGameWork.Tests.Mocks
{
    public class MockPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<Guid, Player> _players = new();

        public MockPlayerRepository AddPlayer(Player player)
        {
            _players[player.Id] = player;
            return this;
        }

        public Task<Player> GetByIdAsync(Guid id) => Task.FromResult(_players.GetValueOrDefault(id));
        public Task<Player> GetById(Guid id) => Task.FromResult(_players.GetValueOrDefault(id));
        public Task AddAsync(Player player) => Task.CompletedTask;

        public Task<Player> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        Player IPlayerRepository.GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
