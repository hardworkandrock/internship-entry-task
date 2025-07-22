namespace TestGameWork.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Размер игрового поля
        /// </summary>
        public int BoardSize { get; set; }
        /// <summary>
        /// Игровое поле
        /// </summary>
        public string BoardState { get; set; } // JSON string
        public Guid Player1 { get; set; }
        public Guid Player2 { get; set; }
        /// <summary>
        /// Номер игрока который совершает текущий ход [1,2]
        /// </summary>
        public int Step { get; set; } = 1;
        /// <summary>
        /// Количество последовательных символов необходимых для победы.
        /// </summary>
        public int WinCondition { get; set; }
        public GameStatus Status { get; set; } = GameStatus.Active;
    }
    /// <summary>
    /// Любой статус > 0 означает конец игры 
    /// Статус игры:
    /// 0 - Игра не закончена
    /// 1 - Победитель Player1
    /// 2 - Победитель Player2
    /// 3 - Игра завершена
    /// </summary>
    public enum GameStatus
    {
        Active,
        XWin,
        OWin,
        Finished
    }
}
