namespace TestGameWork.DTOs
{
    public class GameResponse
    {
        public Guid Id { get; set; }
        public int BoardSize { get; set; } 
        public List<List<string>> Board { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Step { get; set; }
        public string Status { get; set; }
    }
}
