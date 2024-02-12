using Pastel;
using System.CommandLine;
using System.Drawing;

namespace Othello
{
    public class Program
    {
        /// <summary>
        /// Implements the main gameplay loop and logic.
        /// </summary>
        internal class Othello
        {
            public static int MIN_BOARD_SIZE = 4;
            public static int MAX_BOARD_SIZE = 10;
            public static int DEFAULT_BOARD_SIZE = 8;

            private Board board;
            private readonly Player playerBlack;
            private readonly Player playerWhite;
            private int roundsPlayed;
            private int gamesPlayed;
            private readonly Settings settings;
            private readonly List<string> gameLog = new();

            private Othello(Settings settings)
            {
                board = new Board(settings.BoardSize);
                this.settings = settings;
                gamesPlayed = 0;
                playerBlack = Player.Black(settings.ToPlayerSettings());
                playerWhite = Player.White(settings.ToPlayerSettings());
                roundsPlayed = 0;
            }

            /// <summary>
            /// Plays and manages a single full game of Othello.
            /// </summary>
            private void Play()
            {
                while (true)
                {
                    InitGame();
                    GameLoop();
                    PrintResult();
                    if (settings.AutoplayMode || !GetAnswer("Would you like to play again"))
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Initializes the board and players for a new game.
            /// </summary>
            private void InitGame()
            {
                if (gamesPlayed > 0)
                {
                    board = new Board(settings.BoardSize);
                    playerBlack.Reset();
                    playerWhite.Reset();
                    roundsPlayed = 0;
                    gameLog.Clear();
                }

                if (settings.AutoplayMode)
                {
                    // Computer plays both
                    playerWhite.SetHuman(false);
                    playerBlack.SetHuman(false);
                }
                else if (settings.UseDefaults)
                {
                    // Default: play as black against white computer player
                    playerWhite.SetHuman(false);
                }
                else if (GetAnswer("Would you like to play against the computer"))
                {
                    if (GetAnswer("Would you like to play as black or white", "b", "w"))
                    {
                        playerWhite.SetHuman(false);
                    }
                    else
                    {
                        playerBlack.SetHuman(false);
                    }
                }

                Console.WriteLine("\nPlayers:".Pastel(System.Drawing.Color.Silver));
                PrintStatus();
            }

            /// <summary>
            /// Keeps both players making moves until neither can make a move.
            /// </summary>
            private void GameLoop()
            {
                while (board.CanPlay() && (playerBlack.canPlay || playerWhite.canPlay))
                {
                    ++roundsPlayed;
                    Console.WriteLine($"\n=========== ROUND: {roundsPlayed} ===========");
                    foreach (Player player in new Player[] { playerBlack, playerWhite })
                    {
                        var result = player.PlayOneMove(board);
                        if (result != null)
                        {
                            gameLog.Add($"{result};{board.ToLogEntry()}");
                        }
                        Console.WriteLine("--------------------------------");
                    }
                }
                ++gamesPlayed;
            }

            /// <summary>
            /// Prints final board status and winner info.
            /// </summary>
            private void PrintResult()
            {
                Console.WriteLine("\n================================");
                ConsoleVisuals.WriteLine("The game is finished!", System.Drawing.Color.Green);
                Console.WriteLine("Result:".Pastel(System.Drawing.Color.Silver));
                PrintStatus();
                Console.WriteLine("");

                var winner = board.Result();
                if (winner == Color.Empty)
                {
                    Console.WriteLine("The game ended in a tie...\n");
                }
                else
                {
                    Console.WriteLine($"The winner is {winner.Name()}!\n");
                }
            }

            /// <summary>
            /// Prints current board and player info.
            /// </summary>
            private void PrintStatus()
            {
                Console.WriteLine(playerBlack);
                Console.WriteLine(playerWhite);
                Console.WriteLine($"\n{board}");
            }

            /// <summary>
            /// Asks a question with two answer options.
            /// </summary>
            /// <param name="question">String containing the question content.</param>
            /// <param name="yes">String for option 1.</param>
            /// <param name="no">String for option 2.</param>
            /// <returns>Bool from user answer</returns>
            private static bool GetAnswer(string question, string yes = "y", string no = "n")
            {
                Console.Write($"{question} ({yes}/{no})? ");
                var ans = Console.ReadLine();
                return !string.IsNullOrEmpty(ans)
                    && string.Equals(ans, yes, StringComparison.CurrentCultureIgnoreCase);
            }

            /// <summary>
            /// Asks for the desired board size.
            /// </summary>
            /// <returns>Desired board size.</returns>
            private static int GetBoardSize()
            {
                Console.Write($"Choose board size (default is {DEFAULT_BOARD_SIZE}): ");
                if (int.TryParse(Console.ReadLine(), out var boardSize))
                {
                    if (boardSize < Othello.MIN_BOARD_SIZE || boardSize > Othello.MAX_BOARD_SIZE)
                    {
                        ConsoleVisuals.Warn(
                            $"Limiting board size to valid range {Othello.MIN_BOARD_SIZE}...{Othello.MAX_BOARD_SIZE}"
                        );
                    }
                    return Math.Max(
                        Othello.MIN_BOARD_SIZE,
                        Math.Min(boardSize, Othello.MAX_BOARD_SIZE)
                    );
                }
                ConsoleVisuals.Warn($"Invalid size, defaulting to {Othello.DEFAULT_BOARD_SIZE}...");
                return Othello.DEFAULT_BOARD_SIZE;
            }

            public static int Main(string[] args)
            {
                var size = new Argument<int?>(
                    name: "size",
                    description: "Optional board size",
                    getDefaultValue: () => null
                );

                var autoplay = new Option<bool>(
                    name: "--autoplay",
                    description: "Enable autoplay mode"
                );
                autoplay.AddAlias("-a");

                var useDefaultSettings = new Option<bool>(
                    name: "--default",
                    description: "Play with default settings"
                );
                useDefaultSettings.AddAlias("-d");

                var showLog = new Option<bool>(name: "--log", description: "Show log after a game");
                showLog.AddAlias("-l");

                var hideHelpers = new Option<bool>(
                    name: "--no-helpers",
                    description: "Hide disk placement hints"
                );
                hideHelpers.AddAlias("-n");

                var testMode = new Option<bool>(name: "--test", description: "Enable test mode");
                testMode.AddAlias("-t");

                var version = new Option<bool>(name: "-v", description: "Print version and exit");

                var rootCommand = new RootCommand("A simple Othello CLI game implementation")
                {
                    size,
                    autoplay,
                    useDefaultSettings,
                    showLog,
                    hideHelpers,
                    testMode,
                    version
                };

                ConsoleVisuals.WriteLine("OTHELLO GAME - C#", System.Drawing.Color.Green);

                rootCommand.SetHandler(
                    (size, autoplay, useDefaultSettings, showLog, hideHelpers, testMode, version) =>
                    {
                        int boardSize;
                        if (size != null)
                        {
                            boardSize = size.Value;
                            if (boardSize < Othello.MIN_BOARD_SIZE || boardSize > Othello.MAX_BOARD_SIZE)
                            {
                                ConsoleVisuals.Error($"Unsupported board size: {boardSize}");
                                Environment.Exit(1);
                            }
                            Console.WriteLine($"Using board size: {boardSize}");
                        }
                        else if (autoplay || useDefaultSettings)
                        {
                            boardSize = DEFAULT_BOARD_SIZE;
                        }
                        else
                        {
                            // Ask user for board size
                            boardSize = GetBoardSize();
                        }

                        Settings settings =
                            new(boardSize,
                                autoplay,
                                useDefaultSettings,
                                showLog,
                                !hideHelpers,
                                testMode);

                        var game = new Othello(settings);
                        game.Play();
                    },
                    size,
                    autoplay,
                    useDefaultSettings,
                    showLog,
                    hideHelpers,
                    testMode,
                    version
                );

                try
                {
                    return rootCommand.Invoke(args);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\ncancelled...");
                    return 1;
                }
                catch (Exception ex)
                {
                    ConsoleVisuals.Error($"An exception occurred: {ex.Message}");
                    return 1;
                }
            }
        }
    }
}
