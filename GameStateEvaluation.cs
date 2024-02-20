namespace Othello
{
    internal static class GameStateEvaluation
    {
        public static int EvaluateBoard(Board board, Color playerColor)
        {
            int score = 0;

            if (ShouldApplyHeuristic(board, "DiskDifference"))
            {
                int playerDiscs = CountDiscs(board, playerColor);
                int opponentDiscs = CountDiscs(board, playerColor.Opponent());
                score += (playerDiscs - opponentDiscs) * 64;
                // Disc Difference: Varies from -64 to 64 in an 8x8 game (all pieces one color to all pieces the opposite color).
                // Normal range in competitive play is narrower.
            }

            if (ShouldApplyHeuristic(board, "Mobility"))
            {
                int playerMobility = CountMobility(board, playerColor);
                int opponentMobility = CountMobility(board, playerColor.Opponent());
                score += (playerMobility - opponentMobility) / board.GetEmptySquaresCount();
                // Mobility: Can range from 0 (no moves available) to a maximum based on board state.
                // Early game, this is low; midgame, it can be quite high.
            }

            if (ShouldApplyHeuristic(board, "CornerControl"))
            {
                int playerCorners = CountCorners(board, playerColor);
                int opponentCorners = CountCorners(board, playerColor.Opponent());
                score += (playerCorners - opponentCorners) / 4;
                // Corner Control: Ranges from 0 to 4, as there are four corners.
            }

            if (ShouldApplyHeuristic(board, "Stability"))
            {
                int playerStability = CalculateStability(board, playerColor);
                int opponentStability = CalculateStability(board, playerColor.Opponent());
                score += (playerStability - opponentStability) / board.GetPlayerScore(playerColor);
                // Stability: Hard to quantify universally due to its complexity, but you might consider a range based on
                // potentially stable positions available from the current board state.
                // The maximum theoretically occurs when all a player's pieces are stable, which, while rare,
                // could approach the total number of pieces a player has on the board at a given time.
            }

            if (ShouldApplyHeuristic(board, "Parity"))
                score += CalculateParity(board, playerColor);
            // Parity: Often binary (-1, 1) representing disadvantage or advantage, but could be expanded based on remaining move possibilities.

            if (ShouldApplyHeuristic(board, "SquareWeights"))
                score += CalculateSquareWeights(board, playerColor) / (25 * board.size);
            // Square Weights: This depends on your weight distribution but is bounded by the total positive or negative weight
            // assigned to the board configuration. For our function, let's assume 25 * board size.

            return score;
        }

        /// <summary>
        /// Square weights in board game AIs like Othello are not calculated through a mathematical formula, 
        /// but are usually determined based on gameplay experience, strategy, and sometimes machine learning algorithms 
        /// that adjust the weights based on outcomes of many games. 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerColor"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CalculateSquareWeights(Board board, Color playerColor)
        {
            // Define the weights for each square on an 8x8 board
            int[,] squareWeights = GenerateSquareWeights(board.size);

            int score = 0;

            // Loop through all the squares on the board
            for (int x = 0; x < board.size; x++)
            {
                for (int y = 0; y < board.size; y++)
                {
                    Color? squareColor = board.GetSquare(new Square(x, y));
                    if (squareColor == playerColor)
                    {
                        score += squareWeights[x, y];
                    }
                    else if (squareColor == playerColor.Opponent())
                    {
                        score -= squareWeights[x, y];
                    }
                }
            }
            return score;
        }

        /// <summary>
        /// The values in the weight array I provided are an example based on common strategies in Othello.
        /// For instance, corners are valued highly because they provide a stable position that cannot be flipped. Edges are also valuable, 
        /// but not as much as corners.Squares adjacent to corners can be dangerous if taken too early, as they might give the opponent access 
        /// to a corner, which is why they may have negative values.
        /// </summary>
        /// <param name="boardSize"></param>
        /// <returns></returns>
        private static int[,] GenerateSquareWeights(int boardSize)
        {
            int[,] squareWeights = new int[boardSize, boardSize];

            // Define weights for corners, edges, and adjacent to corners
            int cornerWeight = 20;
            int edgeWeight = 5;
            int adjacentToCornerWeight = -5;
            int centerWeight = 0;

            // Set weights for corners
            squareWeights[0, 0] = cornerWeight;
            squareWeights[0, boardSize - 1] = cornerWeight;
            squareWeights[boardSize - 1, 0] = cornerWeight;
            squareWeights[boardSize - 1, boardSize - 1] = cornerWeight;

            // Set weights for edges
            for (int i = 1; i < boardSize - 1; i++)
            {
                squareWeights[0, i] = edgeWeight; // Top edge
                squareWeights[boardSize - 1, i] = edgeWeight; // Bottom edge
                squareWeights[i, 0] = edgeWeight; // Left edge
                squareWeights[i, boardSize - 1] = edgeWeight; // Right edge
            }

            // Set weights for squares adjacent to corners
            squareWeights[1, 0] = adjacentToCornerWeight;
            squareWeights[0, 1] = adjacentToCornerWeight;
            squareWeights[1, 1] = adjacentToCornerWeight;

            squareWeights[boardSize - 2, 0] = adjacentToCornerWeight;
            squareWeights[boardSize - 1, 1] = adjacentToCornerWeight;
            squareWeights[boardSize - 2, 1] = adjacentToCornerWeight;

            squareWeights[0, boardSize - 2] = adjacentToCornerWeight;
            squareWeights[1, boardSize - 1] = adjacentToCornerWeight;
            squareWeights[1, boardSize - 2] = adjacentToCornerWeight;

            squareWeights[boardSize - 1, boardSize - 2] = adjacentToCornerWeight;
            squareWeights[boardSize - 2, boardSize - 1] = adjacentToCornerWeight;
            squareWeights[boardSize - 2, boardSize - 2] = adjacentToCornerWeight;

            // Set weights for all other squares
            for (int i = 1; i < boardSize - 1; i++)
            {
                for (int j = 1; j < boardSize - 1; j++)
                {
                    if (squareWeights[i, j] == 0) // Only set it if it's not an edge or corner adjacent
                    {
                        squareWeights[i, j] = centerWeight;
                    }
                }
            }

            return squareWeights;
        }

        /// <summary>
        /// Parity Based on Remaining Moves: If the number of remaining moves is odd, the player who is currently to move 
        /// has the parity advantage; if even, the parity advantage will go to the other player.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerColor"></param>
        /// <returns></returns>
        private static int CalculateParity(Board board, Color playerColor)
        {
            int totalSquares = board.size * board.size;
            var (black, white) = board.PlayerScores();
            int totalPieces = white + black;
            int remainingMoves = totalSquares - totalPieces;

            // Parity advantage is given a score, e.g., 1 for advantage, -1 for disadvantage
            int parityScore = (remainingMoves % 2 == 0) ? -1 : 1;

            return parityScore;
        }

        /// <summary>
        /// Calculating stability in Othello refers to assessing how many of a player's pieces are stable, 
        /// meaning they cannot be flipped by the opponent in future moves. Stability is a complex heuristic because 
        /// it involves not only the current state but potential future states of the board. A stable piece is one that, 
        /// once placed, cannot be outflanked by any sequence of opponent moves.
        /// 
        /// Corner Stability: Pieces in corners are always stable because they cannot be outflanked.
        /// 
        ///Edge Stability: Pieces on the edges may be stable if there are no breaks in the line of pieces of the same color 
        ///from the edge to the nearest corner or edge of the board.
        ///
        ///Internal Stability: Pieces within the board are stable if, in all four directions (vertical, horizontal, and both diagonals), 
        ///they are part of a line of pieces that either reaches the edge of the board or is blocked on both ends by pieces of the same color.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerColor"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CalculateStability(Board board, Color playerColor)
        {
            int stabilityScore = 0;

            // Evaluate corner stability
            Square[] corners = { new Square(0, 0), new Square(0, board.size - 1), new Square(board.size - 1, 0), new Square(board.size - 1, board.size - 1) };
            foreach (var corner in corners)
            {
                if (board.GetSquare(corner) == playerColor)
                {
                    stabilityScore += 1; // Increment stability score for each stable corner
                }
            }

            // Simplified edge stability (this is a very naive approach and should be refined)
            for (int x = 0; x < board.size; x++)
            {
                if (board.GetSquare(new Square(x, 0)) == playerColor) stabilityScore++;
                if (board.GetSquare(new Square(x, board.size - 1)) == playerColor) stabilityScore++;
            }
            for (int y = 1; y < board.size - 1; y++)
            {
                if (board.GetSquare(new Square(0, y)) == playerColor) stabilityScore++;
                if (board.GetSquare(new Square(board.size - 1, y)) == playerColor) stabilityScore++;
            }

            // Ideally, add more sophisticated checks for edge and internal stability here

            return stabilityScore;
        }

        private static int CountCorners(Board board, Color playerColor)
        {
            int count = 0;
            // Define the coordinates of the four corners
            Square[] corners = { new Square(0, 0), new Square(0, board.size - 1), new Square(board.size - 1, 0), new Square(board.size - 1, board.size - 1) };

            // Check each corner to see if it is occupied by the player's disc.
            foreach (var corner in corners)
                if (board.GetSquare(corner) == playerColor)
                    count++;

            return count;
        }

        private static int CountMobility(Board board, Color playerColor)
        {
            var possibleMoves = board.PossibleMoves(playerColor);
            return possibleMoves.Count;
        }


        private static int CountDiscs(Board board, Color playerColor)
        {
            var (black, white) = board.PlayerScores();
            int count = playerColor == Color.White ? white : black;
            return count;
        }


        public static bool ShouldApplyHeuristic(Board board, string heuristicName)
        {
            var (black, white) = board.PlayerScores();
            int diskCount = black + white;
            int midgameThreshold = board.size * board.size / 3;
            int endgameThreshold = (int)Math.Floor(board.size * board.size * 0.96);

            switch (heuristicName)
            {
                case "DiskDifference":
                    return diskCount > midgameThreshold;
                case "Mobility":
                    return diskCount < endgameThreshold;
                case "CornerControl":
                    return true;
                case "Stability":
                    return true;
                case "Parity":
                    return diskCount > midgameThreshold;
                case "SquareWeights":
                    return diskCount < endgameThreshold;
                default:
                    return false;
            }
        }
    }
}
