using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGameWork.DTOs;
using TestGameWork.Models;
using TestGameWork.Repositories;
using TestGameWork.Services;

namespace TestGameWork.Tests.UTests
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock = new();
        private readonly Mock<IPlayerRepository> _playerRepoMock = new();
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<IConfigurationSection> _configSectionMock = new();

        private GameService CreateGameService()
        {
            _configMock.Setup(c => c.GetSection("AppSettings:BoardSize")).Returns(_configSectionMock.Object);
            _configMock.Setup(c => c.GetSection("AppSettings:WinCondition")).Returns(_configSectionMock.Object);
            _configSectionMock.Setup(c => c.Value).Returns("3");

            return new GameService(_gameRepoMock.Object, _playerRepoMock.Object, _configMock.Object);
        }

        /// <summary>
        /// Игра создана
        /// Стандартное поле = 3
        /// Игрок1 создавший игру установлен верно
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateGameAsync_CreatesGameWithCorrectDefaults()
        {
            // Arrange
            var service = CreateGameService();
            var users = new[]
            {
                new Player { Id= Guid.NewGuid(), Name = "Player1" },
                new Player { Id= Guid.NewGuid(), Name = "Player2" },
                new Player { Id= Guid.NewGuid(), Name = "Player3" },
                new Player { Id= Guid.NewGuid(), Name = "Player4" }
            };
            var request = new CreateGameRequest();
               
            _playerRepoMock.Setup(p => p.GetById(It.IsAny<Guid>()))
                          .Returns((Guid id) => users.FirstOrDefault(u => u.Id == id));

            // Act
            var result = await service.CreateGameAsync(request, users[3].Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.BoardSize);
            Assert.Equal(users[3].Name.ToString(), result.Player1);
        }

        /// <summary>
        /// Ход игрока успешно завершен
        /// Символ хода первого игрока корректный
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MakeMoveAsync_MakeValidMoveAndReturnUpdatedGame()
        {
            // Arrange
            var service = CreateGameService();
            var game = new Game
            {
                Id = Guid.NewGuid(),
                BoardSize = 3,
                WinCondition = 3,
                Step = 1,
                Player1 = Guid.NewGuid(),
                Player2 = Guid.NewGuid(),
                BoardState = "[[null,null,null],[null,null,null],[null,null,null]]"
            };

            var userId = game.Player1;
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
            _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
              
            var moveRequest = new MakeMoveRequest { Row = 0, Column = 0 };

            // Act
            var result = await service.MakeMoveAsync(game.Id, moveRequest, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("X", result.Board[0][0]);
        }

        /// <summary>
        /// Вызов исключения: Ячейка занята
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MakeMoveAsync_ThrowsIfCellTaken()
        {
            // Arrange
            var service = CreateGameService();
            var game = new Game
            {
                Id = Guid.NewGuid(),
                BoardSize = 3,
                WinCondition = 3,
                Step = 1,
                Player1 = Guid.NewGuid(),
                Player2 = Guid.NewGuid(),
                BoardState = "[[\"X\",null,null],[null,null,null],[null,null,null]]"
            };
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
              
            var moveRequest = new MakeMoveRequest { Row = 0, Column = 0 };
            
            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.MakeMoveAsync(game.Id, moveRequest, game.Player1));
        }

        /// <summary>
        /// Победный шаг. Игра меняет статус
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MakeMoveAsync_CheckWin()
        {
            // Arrange 
            var service = CreateGameService();
            var game = new Game
            {
                Id = Guid.NewGuid(),
                BoardSize = 3,
                WinCondition = 3,
                Step = 1,
                Player1 = Guid.NewGuid(),
                Player2 = Guid.NewGuid(),
                BoardState = "[[\"X\",null,\"O\"],[\"X\",\"O\",null],[null,null,null]]"
            };
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game); 
              
            var moveRequest = new MakeMoveRequest { Row = 2, Column = 0 };

            // Assert
            var resp = await service.MakeMoveAsync(game.Id, moveRequest, game.Player1);
            Assert.NotEqual(resp.Status, GameStatus.Active.ToString()); 
        }

        /// <summary>
        /// Вызов исключения: Ход другого игрока
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MakeMoveAsync_CheckStep()
        {
            // Arrange
            var service = CreateGameService();
            var game = new Game
            {
                Id = Guid.NewGuid(),
                BoardSize = 3,
                WinCondition = 3,
                Step = 2,
                Player1 = Guid.NewGuid(),
                Player2 = Guid.NewGuid(),
                BoardState = "[[\"X\",null,null],[\"X\",null,null],[null,null,null]]"
            };
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game); 

            var moveRequest = new MakeMoveRequest { Row = 2, Column = 0 };

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
               service.MakeMoveAsync(game.Id, moveRequest, game.Player1));
        }

        /// <summary>
        /// Получение списка игр
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetListAsync_ReturnsAllGamesForUser()
        {
            // Arrange 
            var service = CreateGameService();
            var users = new[]
            {
                new Player { Id= Guid.NewGuid(), Name = "Player1" },
                new Player { Id= Guid.NewGuid(), Name = "Player2" },
                new Player { Id= Guid.NewGuid(), Name = "Player3" },
                new Player { Id= Guid.NewGuid(), Name = "Player4" }
            };
            var games = new List<Game>
            {
                new Game { Id = Guid.NewGuid(), Player1 = users[0].Id, Player2 = users[1].Id },
                new Game { Id = Guid.NewGuid(), Player1 = users[2].Id, Player2 = users[0].Id }
            };
             
            _gameRepoMock.Setup(r => r.GetAllAsync(users[0].Id)).ReturnsAsync(games);
             
            _playerRepoMock.Setup(p => p.GetById(It.IsAny<Guid>()))
                          .Returns((Guid id) => users.FirstOrDefault(u => u.Id == id)); 
              
            // Act
            var result = await service.GetListAsync(users[0].Id);

            // Assert
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Добавить второго игрока в игру
        /// Проверка имени второго игрока
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EnterTheGame_AddsSecondPlayer()
        {
            // Arrange 
            var service = CreateGameService();
            var users = new[]
            {
                new Player { Id= Guid.NewGuid(), Name = "Player1" },
                new Player { Id= Guid.NewGuid(), Name = "Player2" },
                new Player { Id= Guid.NewGuid(), Name = "Player3" },
                new Player { Id= Guid.NewGuid(), Name = "Player4" }
            };
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Player1 = users[0].Id, 
                Player2 = Guid.Empty
            }; 
            var secondPlayer = users[3];
             
            _playerRepoMock.Setup(p => p.GetById(It.IsAny<Guid>()))
                          .Returns((Guid id) => users.FirstOrDefault(u => u.Id == id));
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game); 
             
            // Act
            var result = await service.EnterTheGame(game.Id, secondPlayer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secondPlayer.Name, result.Player2);
        }

        /// <summary>
        /// Игра закончена досрочно
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FinishTheGameAsync_SetsGameToFinished()
        {
            // Arrange
            var service = CreateGameService();
            var users = new[]
            {
                new Player { Id= Guid.NewGuid(), Name = "Player1" },
                new Player { Id= Guid.NewGuid(), Name = "Player2" },
                new Player { Id= Guid.NewGuid(), Name = "Player3" },
                new Player { Id= Guid.NewGuid(), Name = "Player4" }
            };
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Player1 = Guid.NewGuid(),
                Player2 = Guid.NewGuid(),
                Status = GameStatus.Active
            };
             
            _playerRepoMock.Setup(p => p.GetById(It.IsAny<Guid>()))
                          .Returns((Guid id) => users.FirstOrDefault(u => u.Id == id));
             
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
            _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
             
            // Act
            await service.FinishTheGameAsync(game.Id, game.Player1);

            // Assert
            Assert.Equal(GameStatus.Finished, game.Status);
        }
    }
}
