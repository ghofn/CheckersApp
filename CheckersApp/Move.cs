namespace CheckersApp
{
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public Position CapturedPiece { get; set; }
        public bool IsCapture { get; set; }

        public Move(Position from, Position to)
        {
            From = from;
            To = to;
            IsCapture = false;
        }

        public Move(Position from, Position to, Position capturedPiece)
        {
            From = from;
            To = to;
            IsCapture = true;
            CapturedPiece = capturedPiece;
        }

        public override string ToString()
        {
            return $"Move: ({From.Row},{From.Column}) -> ({To.Row},{To.Column})";
        }
    }

    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    public enum PieceColor
    {
        White,
        Black
    }

    public enum GameMode
    {
        TwoPlayers,
        PlayerVsAI
    }
}