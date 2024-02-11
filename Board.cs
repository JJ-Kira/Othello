using System.Drawing;
using System.Text;

namespace Othello
{
    /// <summary>
    /// Handles board state and logic
    /// </summary>
    internal class Board
    {
        private readonly List<Piece> _board;
        private readonly List<Square> _emptySquares;
        private readonly List<int> _indices;
        private readonly int _size;

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
            _size = size;
            var numSquares = _size * _size;
            // init game board with empty disks
            _board = Enumerable.Repeat(Piece.Empty, numSquares).ToList();

            // set starting positions
            var row = _size % 2 == 0 ? (_size - 1) / 2 : (_size - 1) / 2 - 1;
            var col = _size / 2;
            _board[row * _size + row] = Piece.White;
            _board[row * _size + col] = Piece.Black;
            _board[col * _size + row] = Piece.Black;
            _board[col * _size + col] = Piece.White;

            // index list (0...size) to avoid repeating same range in for loops
            _indices = Enumerable.Range(0, _size).ToList();

            // keep track of empty squares on board to avoid checking already filled positions
            _emptySquares = new List<Square>(numSquares);
            foreach (
                var square in _indices
                    .SelectMany(y => _indices, (y, x) => new Square(x, y))
                    .Where(square => GetSquare(square) == Piece.Empty)
            )
            {
                _emptySquares.Add(square);
            }
        }

        /// <summary>
        /// Checks if it i still possible to make a move.
        /// </summary>
        /// <returns>True if board contains empty squares.</returns>
        public bool CanPlay()
        {
            return _emptySquares.Count != 0;
        }

        /// <summary>
        /// Updates board for the given piece placement.
        /// </summary>
        /// <param name="move"></param>
        /// <exception cref="ArgumentException"></exception>
        public void PlaceDisc(Move move)
        {
            var start = move.Square;
            if (GetSquare(start) != Piece.Empty)
            {
                throw new ArgumentException($"Trying to place disk to an occupied square {start}!");
            }
            SetSquare(start, move.Disk);
            _emptySquares.Remove(start);
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
        public List<Move> PossibleMoves(Piece color)
        {
            var moves = new List<Move>();
            var other = color.Opponent();
            foreach (var square in _emptySquares)
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
        public void printPossibleMoves(IReadOnlyCollection<Move> moves)
        {
            ConsoleManager.WriteLine($"  Possible moves ({moves.Count}):", Color.Yellow);
            // convert board from Disk enums to strings
            var formattedBoard = new List<string>(_board.Count);
            formattedBoard.AddRange(_board.Select(disk => disk.BoardChar()));
            foreach (var move in moves)
            {
                Console.WriteLine($"  {move}");
                var (x, y) = move.Square;
                formattedBoard[y * _size + x] = ConsoleManager.Get(move.Value, Color.Yellow);
            }
            // print board with move positions
            Console.Write("   ");
            foreach (var i in _indices)
            {
                Console.Write($" {i}");
            }
            foreach (var y in _indices)
            {
                Console.Write($"\n  {y}");
                foreach (var x in _indices)
                {
                    Console.Write($" {formattedBoard[y * _size + x]}");
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
                $"Score: {ConsoleManager.Get(black, Piece.Black.DiskColor())} | "
                    + $"{ConsoleManager.Get(white, Piece.White.DiskColor())}"
            );
        }

        /// <summary>
        /// Calculates the final score.
        /// </summary>
        /// <returns>The winning player.</returns>
        public Piece Result()
        {
            var sum = Score();
            if (sum == 0)
            {
                return Piece.Empty;
            }
            return sum > 0 ? Piece.White : Piece.Black;
        }

        public string ToLogEntry()
        {
            return _board.Aggregate(
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
            return 0 <= x && x < _size && 0 <= y && y < _size;
        }

        /// <summary>
        /// Counts the number of black and white disks.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private (int, int) PlayerScores()
        {
            var black = 0;
            var white = 0;
            foreach (var disk in _board)
            {
                switch (disk)
                {
                    case Piece.White:
                        ++white;
                        break;
                    case Piece.Black:
                        ++black;
                        break;
                    case Piece.Empty:
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
            return _board.Sum(x => Convert.ToInt32(x));
        }

        /// <summary>
        /// Returns the state of the board (empty, white, black) at the given coordinates.
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        private Piece? GetSquare(Square square)
        {
            var (x, y) = square;
            if (!CheckCoordinates(x, y))
            {
                return null;
            }
            return _board[y * _size + x];
        }

        /// <summary>
        /// Sets the square's value.
        /// </summary>
        /// <param name="square"></param>
        /// <param name="pieceStatus"></param>
        /// <exception cref="ArgumentException"></exception>
        private void SetSquare(Square square, Piece pieceStatus)
        {
            var (x, y) = square;
            if (!CheckCoordinates(x, y))
            {
                throw new ArgumentException($"Invalid coordinates ({x},{y})!");
            }
            _board[y * _size + x] = pieceStatus;
        }

        /// Format game board to string
        public override string ToString()
        {
            var text = _indices.Aggregate(" ", (current, i) => current + $" {i}");
            foreach (var y in _indices)
            {
                text += $"\n{y}";
                text = _indices
                    .Select(x => _board[y * _size + x])
                    .Aggregate(text, (current, disk) => current + $" {disk.BoardChar()}");
            }
            return text;
        }
    }
}
