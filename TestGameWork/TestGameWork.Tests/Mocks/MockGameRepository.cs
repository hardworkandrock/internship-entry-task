using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGameWork.Models;
using TestGameWork.Repositories; 

namespace TestGameWork.Tests.Mocks
{
    internal class MockGameRepository : IGameRepository
    {
        private readonly List<Game> _games = new();
         
        public Task AddAsync(Game game)
        {
            _games.Add(game);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Game game)
        {
            var index = _games.FindIndex(g => g.Id == game.Id);
            if (index >= 0)
                _games[index] = game;
            return Task.CompletedTask;
        }

        public Task<Game> GetByIdAsync(Guid id) =>
            Task.FromResult(_games.FirstOrDefault(g => g.Id == id));

        public Task<List<Game>> GetAllAsync(Guid userId) =>
            Task.FromResult(_games.Where(g => g.Player1 == userId || g.Player2 == userId).ToList());


        public Task<List<Game>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
