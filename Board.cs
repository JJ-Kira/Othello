using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Othello
{
    /// <summary>
    /// Handles board state and logic
    /// </summary>
    internal class Board : ICloneable
    {
        private List<Color> board;
        private List<Square> emptySquares;
        private List<int> indices;
        public int size;

        private static readonly Step[] StepDirections =
        {
            new(-1, -1),
            new(-1, 0),
            new(-1, 1),
            new(0, -1),
            new(0, 1),
            new(1, -1),
            new(1, 0),
            new(1, 1)
        };

        public Board(int size)
        {
            this.size = size;
            var numSquares = this.size * this.size;
            // init game board with empty disks
            board = Enumerable.Repeat(Color.Empty, numSquares).ToList();

            // set starting positions
            var row = this.size % 2 == 0 ? (this.size - 1) / 2 : (this.size - 1) / 2 - 1;
            var col = this.size / 2;
            board[row * this.size + row] = Color.White;
            board[row * this.size + col] = Color.Black;
            board[col * this.size + row] = Color.Black;
            board[col * this.size + col] = Color.White;

            // index list (0...size) to avoid repeating same range in for loops
            indices = Enumerable.Range(0, size).ToList();

            // keep track of empty squares on board to avoid checking already filled positions
            emptySquares = new List<Square>(numSquares);
            foreach (
                var square in indices
                    .SelectMany(y => indices, (y, x) => new Square(x, y))
                    .Where(square => GetSquare(square) == Color.Empty)
            )
            {
                emptySquares.Add(square);
            }
        }

        /// <summary>
        /// Checks if it i still possible to make a move.
        /// </summary>
        /// <returns>True if board contains empty squares.</returns>
        public bool CanPlay()
        {
            return emptySquares.Count != 0;
        }

        /// <summary>
        /// Updates board for the given piece placement.
        /// </summary>
        /// <param name="move"></param>
        /// <exception cref="ArgumentException"></exception>
        public void PlacePiece(Move move)
        {
            var start = move.Square;
            if (GetSquare(start) != Color.Empty)
            {
                throw new ArgumentException($"Trying to place disk to an occupied square {start}!");
            }
            SetSquare(start, move.Disk);
            emptySquares.Remove(start);
            foreach (var dir in move.Directions)
            {
                var pos = start + dir;
                while (GetSquare(pos) == move.Disk.Opponent())
                {
                    SetSquare(pos, move.Disk);
                    pos += dir;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns>A list of possible moves for given player.</returns>
        public List<Move> PossibleMoves(Color color)
        {
            var moves = new List<Move>();
            var other = color.Opponent();
            foreach (var square in emptySquares)
            {
                var value = 0;
                var directions = new List<Step>();
                foreach (var dir in StepDirections)
                {
                    var step = new Square(dir.X, dir.Y);
                    var pos = square + step;
                    // next square in this directions needs to be opponents disk
                    if (GetSquare(pos) != other)
                    {
                        continue;
                    }
                    var steps = 0;
                    // keep stepping forward while opponents disks are found
                    while (GetSquare(pos) == other)
                    {
                        ++steps;
                        pos += step;
                    }
                    // valid move if a line of opponents disks ends in own disk
                    if (GetSquare(pos) != color)
                    {
                        continue;
                    }
                    value += steps;
                    directions.Add(dir);
                }
                if (value > 0)
                {
                    moves.Add(new Move(square, value, color, directions));
                }
            }
            if (moves.Count != 0)
            {
                moves.Sort();
            }
            return moves;
        }

        /// Print available move coordinates and resulting points gained.
        public void PrintPossibleMoves(IReadOnlyCollection<Move> moves)
        {
            ConsoleVisuals.WriteLine($"  Possible moves ({moves.Count}):", System.Drawing.Color.Yellow);
            // convert board from Disk enums to strings
            var formattedBoard = new List<string>(board.Count);
            formattedBoard.AddRange(board.Select(disk => disk.BoardChar()));
            foreach (var move in moves)
            {
                Console.WriteLine($"  {move}");
                var (x, y) = move.Square;
                formattedBoard[y * size + x] = ConsoleVisuals.Get(move.Value, System.Drawing.Color.Yellow);
            }
            // print board with move positions
            Console.Write("   ");
            foreach (var i in indices)
            {
                Console.Write($" {i}");
            }
            foreach (var y in indices)
            {
                Console.Write($"\n  {y}");
                foreach (var x in indices)
                {
                    Console.Write($" {formattedBoard[y * size + x]}");
                }
            }
            Console.WriteLine("");
        }

        /// <summary>
        /// Prints current score for both players.
        /// </summary>
        public void PrintScore()
        {
            var (black, white) = PlayerScores();
            Console.WriteLine($"\n{this}");
            Console.WriteLine(
                $"Score: {ConsoleVisuals.Get(black, Color.Black.DiskColor())} | "
                    + $"{ConsoleVisuals.Get(white, Color.White.DiskColor())}"
            );
        }

        /// <summary>
        /// Calculates the final score.
        /// </summary>
        /// <returns>The winning player.</returns>
        public Color Result()
        {
            var sum = Score();
            if (sum == 0)
            {
                return Color.Empty;
            }
            return sum > 0 ? Color.White : Color.Black;
        }

        public string ToLogEntry()
        {
            return board.Aggregate(
                new StringBuilder(),
                (accumulator, disk) => accumulator.Append(disk.BoardChar(false)),
                accumulator => accumulator.ToString()
            );
        }

        /// <summary>
        /// Checks if the given coordinates are inside the board.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CheckCoordinates(int x, int y)
        {
            return 0 <= x && x < size && 0 <= y && y < size;
        }

        /// <summary>
        /// Counts the number of black and white disks.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public (int, int) PlayerScores()
        {
            var black = 0;
            var white = 0;
            foreach (var disk in board)
            {
                switch (disk)
                {
                    case Color.White:
                        ++white;
                        break;
                    case Color.Black:
                        ++black;
                        break;
                    case Color.Empty:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return (black, white);
        }

        /// <summary>
        /// Returns the total score (positive means more white disks and negative means more black disks).
        /// </summary>
        /// <returns></returns>
        private int Score()
        {
            return board.Sum(x => Convert.ToInt32(x));
        }

        /// <summary>
        /// Returns the state of the board (empty, white, black) at the given coordinates.
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public Color? GetSquare(Square square)
        {
            var (x, y) = square;
            if (!CheckCoordinates(x, y))
            {
                return null;
            }
            return board[y * size + x];
        }

        /// <summary>
        /// Sets the square's value.
        /// </summary>
        /// <param name="square"></param>
        /// <param name="pieceStatus"></param>
        /// <exception cref="ArgumentException"></exception>
        private void SetSquare(Square square, Color pieceStatus)
        {
            var (x, y) = square;
            if (!CheckCoordinates(x, y))
            {
                throw new ArgumentException($"Invalid coordinates ({x},{y})!");
            }
            board[y * size + x] = pieceStatus;
        }

        /// Format game board to string
        public override string ToString()
        {
            var text = indices.Aggregate(" ", (current, i) => current + $" {i}");
            foreach (var y in indices)
            {
                text += $"\n{y}";
                text = indices
                    .Select(x => board[y * size + x])
                    .Aggregate(text, (current, disk) => current + $" {disk.BoardChar()}");
            }
            return text;
        }

        public object Clone()
        {
            var clonedBoard = new Board(size);

            clonedBoard.board = new List<Color>(board);

            clonedBoard.emptySquares = new List<Square>(emptySquares);
            return clonedBoard;
        }
    }
}
