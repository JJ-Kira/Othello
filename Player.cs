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
        private readonly Color color;
        private readonly PlayerSettings settings;

        public Player(Color color, PlayerSettings settings)
        {
            canPlay = true;
            this.color = color;
            isHuman = true;
            this.settings = settings;
        }

        /// <summary>
        /// Initializes a new player with black pieces.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>Black player.</returns>
        public static Player Black(PlayerSettings settings)
        {
            return new Player(Color.Black, settings);
        }

        /// <summary>
        /// Initializes a new player with white pieces.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>White player.</returns>
        public static Player White(PlayerSettings settings)
        {
            return new Player(Color.White, settings);
        }

#nullable enable
        /// <summary>
        /// Plays one round of the given player.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public string? PlayOneMove(Board board)
        {
            Console.WriteLine($"Turn: {color.Name()}");
            var moves = board.PossibleMoves(color);
            if (moves.Count != 0)
            {
                canPlay = true;
                if (isHuman && settings.ShowHelpers)
                {
                    board.PrintPossibleMoves(moves);
                }
                var chosenMove = isHuman ? GetHumanMove(moves) : GetComputerMove(moves, board);
                board.PlacePiece(chosenMove);
                board.PrintScore();
                ++roundsPlayed;
                if (!settings.TestMode)
                {
                    Thread.Sleep(1000);
                }
                return chosenMove.ToLogEntry();
            }

            canPlay = false;
            ConsoleVisuals.WriteLine("  No moves available...", System.Drawing.Color.Yellow);
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
        /// Determines the best move for the computer player using the Alpha-Beta pruning algorithm.
        /// </summary>
        /// <param name="moves">The list of possible moves.</param>
        /// <param name="board">The current state of the board.</param>
        /// <returns>The best move determined for the computer player.</returns>
        private Move GetComputerMove(IReadOnlyList<Move> moves, Board board)
        {
            Console.WriteLine("  Computer plays...");

            // Alpha represents the best score that the maximizing player can guarantee at current or higher levels.
            float alpha = float.MinValue;
            // Beta represents the best score that the minimizing player can guarantee at current or higher levels.
            float beta = float.MaxValue;
            // Best move found for the computer.
            Move bestMove = moves[0];
            // Best score found for the computer, initialized to the lowest possible value.
            float bestScore = float.MinValue;
            // The depth of the search tree to explore.
            int depth = 5;

            // Loop through all possible moves to find the best one.
            foreach (var move in moves)
            {
                // Create a deep clone of the board to simulate the move without affecting the actual game board.
                Board newBoard = (Board)board.Clone();
                // Apply the move to the cloned board.
                newBoard.PlacePiece(move);
                // Perform the Alpha-Beta search recursively to evaluate the move.
                float score = AlphaBeta(newBoard, depth - 1, alpha, beta, false);

                // If the move has a better score than the best found so far, update the best move and score.
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                // Update alpha if a better score is found for the maximizing player.
                alpha = Math.Max(alpha, score);
            }
            Console.WriteLine($"  {bestMove.Square} -> {bestMove.Value}");
            return bestMove;
        }

        /// <summary>
        /// Alpha-Beta pruning algorithm to evaluate the best score that can be achieved from the current board state.
        /// </summary>
        /// <param name="board">The board to evaluate.</param>
        /// <param name="depth">The depth of the search tree to explore.</param>
        /// <param name="alpha">The best score the maximizing player can guarantee so far.</param>
        /// <param name="beta">The best score the minimizing player can guarantee so far.</param>
        /// <param name="maximizingPlayer">Whether the current turn is for the maximizing player.</param>
        /// <returns>The best score that can be achieved from the current board state.</returns>
        private float AlphaBeta(Board board, int depth, float alpha, float beta, bool maximizingPlayer)
        {
            // Base case: if we have reached the search tree's depth limit or no moves are possible.
            if (depth == 0 || !board.CanPlay())
            {
                // Call EvaluateBoard to get the heuristic value of the board state
                return GameStateEvaluation.EvaluateBoard(board, maximizingPlayer ? color : color.Opponent());
            }

            if (maximizingPlayer)
            {
                // Best evaluation for maximizing player, starts at the worst case (lowest value).
                float maxEval = int.MinValue;
                // Consider all possible moves for the maximizing player.
                foreach (var move in board.PossibleMoves(color))
                {
                    // Simulate the move on a clone of the board.
                    Board newBoard = (Board)board.Clone();
                    newBoard.PlacePiece(move);
                    // Recursively perform Alpha-Beta pruning to evaluate the move.
                    float eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false);
                    // Find the best evaluation value.
                    maxEval = Math.Max(maxEval, eval);
                    // Update alpha if a better evaluation is found.
                    alpha = Math.Max(alpha, eval);
                    // Alpha-Beta pruning condition: stop evaluating if we find a move that's worse than
                    // the best option for the minimizing player.
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                // Best evaluation for minimizing player, starts at the worst case (highest value).
                float minEval = int.MaxValue;
                // Consider all possible moves for the minimizing player.
                foreach (var move in board.PossibleMoves(color.Opponent()))
                {
                    // Simulate the move on a clone of the board.
                    Board newBoard = (Board)board.Clone();
                    newBoard.PlacePiece(move);
                    // Recursively perform Alpha-Beta pruning to evaluate the move.
                    float eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true);
                    // Find the best evaluation value.
                    minEval = Math.Min(minEval, eval);
                    // Update beta if a better evaluation is found.
                    beta = Math.Min(beta, eval);
                    // Alpha-Beta pruning condition: stop evaluating if we find a move that's worse than
                    // the best option for the minimizing player.
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
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
                ConsoleVisuals.Error($"  Can't place a {color.Name()} disk in square {square}!");
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
                    ConsoleVisuals.Error("  Give coordinates in the form 'x,y'");
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
            return $"{color.Name()} | {TypeString()} | Moves: {roundsPlayed}";
        }
    }
}
