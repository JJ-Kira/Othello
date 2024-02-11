using System.Drawing;

namespace Othello
{
    /// <summary>
    /// Defines a single player (human or computer).
    /// </summary>
    internal class Player
    {
        public bool canPlay;

        private bool isHuman;
        private int roundsPlayed;
        private readonly Piece disk;
        private readonly Random random;
        private readonly PlayerSettings settings;

        public Player(Piece color, PlayerSettings settings)
        {
            canPlay = true;
            disk = color;
            isHuman = true;
            random = new Random();
            this.settings = settings;
        }

        /// <summary>
        /// Initializes a new player with black pieces.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>Black player.</returns>
        public static Player Black(PlayerSettings settings)
        {
            return new Player(Piece.Black, settings);
        }

        /// <summary>
        /// Initializes a new player with white pieces.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>White player.</returns>
        public static Player White(PlayerSettings settings)
        {
            return new Player(Piece.White, settings);
        }

#nullable enable
        /// <summary>
        /// Plays one round of the given player.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public string? PlayOneMove(Board board)
        {
            Console.WriteLine($"Turn: {disk.Name()}");
            var moves = board.PossibleMoves(disk);
            if (moves.Count != 0)
            {
                canPlay = true;
                if (isHuman && settings.ShowHelpers)
                {
                    board.printPossibleMoves(moves);
                }
                var chosenMove = isHuman ? GetHumanMove(moves) : GetComputerMove(moves);
                board.PlaceDisc(chosenMove);
                board.PrintScore();
                ++roundsPlayed;
                if (!settings.TestMode)
                {
                    Thread.Sleep(1000);
                }
                return chosenMove.ToLogEntry();
            }

            canPlay = false;
            ConsoleManager.WriteLine("  No moves available...", Color.Yellow);
            return null;
        }

#nullable disable

        /// <summary>
        /// Sets the player to be controlled by human or computer.
        /// </summary>
        /// <param name="isHuman"></param>
        public void SetHuman(bool isHuman)
        {
            this.isHuman = isHuman;
        }

        /// <summary>
        /// Resets the player's status for a new game.
        /// </summary>
        public void Reset()
        {
            canPlay = true;
            roundsPlayed = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        /// <returns>Move chosen by computer.</returns>
        private Move GetComputerMove(IReadOnlyList<Move> moves)
        {
            Console.WriteLine("  Computer plays...");
            Move chosenMove;
            if (settings.TestMode)
            {
                chosenMove = moves[0];
            }
            else
            {
                // Wait a bit and pick a random move
                Thread.Sleep(random.Next(1000, 2000));
                chosenMove = moves[random.Next(moves.Count)];
            }
            Console.WriteLine($"  {chosenMove.Square} -> {chosenMove.Value}");
            return chosenMove;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        /// <returns>Move chosen by a human player.</returns>
        private Move GetHumanMove(List<Move> moves)
        {
            while (true)
            {
                var square = GetSquare();
                // check if given square is one of the possible moves
                if (moves.Exists(x => square.Equals(x.Square)))
                {
                    return moves.Find(x => square.Equals(x.Square));
                }
                ConsoleManager.Error($"  Can't place a {disk.Name()} disk in square {square}!");
            }
        }

        /// <summary>
        /// Asks human player for square coordinates.
        /// </summary>
        /// <returns>The chosen square coordinates.</returns>
        private static Square GetSquare()
        {
            while (true)
            {
                try
                {
                    Console.Write("  Give disk position (x,y): ");
                    var coords = Console.ReadLine();
                    if (string.IsNullOrEmpty(coords) || coords.Length != 3 || coords[1] != ',')
                    {
                        throw new FormatException("Invalid coordinates");
                    }
                    var x = int.Parse(coords[0..1]);
                    var y = int.Parse(coords[2..3]);
                    return new Square(x, y);
                }
                catch (FormatException)
                {
                    ConsoleManager.Error("  Give coordinates in the form 'x,y'");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Player type description string</returns>
        private string TypeString()
        {
            return isHuman ? "Human   " : "Computer";
        }

        public override string ToString()
        {
            return $"{disk.Name()} | {TypeString()} | Moves: {roundsPlayed}";
        }
    }
}
