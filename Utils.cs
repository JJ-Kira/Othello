using System.Drawing;

namespace Othello
{
    /// <summary>
    /// Represents a game piece of given color or lack of one.
    /// </summary>
    public enum Color : int
    {
        Black = -1,
        Empty = 0,
        White = 1
    }

    /// <summary>
    /// Represents a single step direction on the board.
    /// </summary>
    public readonly struct Step
    {
        public readonly int X;
        public readonly int Y;

        public Step(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public bool Equals(Step other)
        {
            var (x, y) = other;
            return X.Equals(x) && Y.Equals(y);
        }

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static bool operator ==(Step left, Step right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Step left, Step right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Represents a single square location on the board.
    /// </summary>
    public readonly struct Square : IComparable<Square>
    {
        public readonly int X;
        public readonly int Y;

        public Square(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Square operator +(Square square, Step step)
        {
            return new Square(square.X + step.X, square.Y + step.Y);
        }

        public bool Equals(Square other)
        {
            var (x, y) = other;
            return X.Equals(x) && Y.Equals(y);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public int CompareTo(Square other)
        {
            if (X == other.X)
            {
                return Y.CompareTo(other.Y);
            }

            return X.CompareTo(other.X);
        }

        public static Square operator +(Square left, Square right)
        {
            return new Square(left.X + right.X, left.Y + right.Y);
        }

        public static bool operator ==(Square left, Square right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Square left, Square right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Square left, Square right)
        {
            return left.X < right.X || (left.X <= right.X && left.Y < right.Y);
        }

        public static bool operator >(Square left, Square right)
        {
            return left.X > right.X || (left.X >= right.X && left.Y > right.Y);
        }
    }

    /// <summary>
    /// Represents a possible disk placement for the given disk color.
    /// </summary>
    public readonly struct Move : IComparable<Move>
    {
        public readonly Square Square;
        public readonly int Value;
        public readonly Color Disk;
        public readonly List<Step> Directions;

        public Move(Square square, int value, Color disk, List<Step> directions)
        {
            Square = square;
            Value = value;
            Disk = disk;
            Directions = directions;
        }

        public string ToLogEntry()
        {
            return $"{Disk.BoardChar(false)}:{Square},{Value}";
        }

        public override string ToString()
        {
            return $"Square: {Square} -> value: {Value}";
        }

        public int CompareTo(Move other)
        {
            var value = other.Value.CompareTo(Value);
            return value == 0 ? Square.CompareTo(other.Square) : value;
        }

        public static bool operator <(Move left, Move right)
        {
            return left.Value > right.Value
                || (left.Value == right.Value && left.Square < right.Square);
        }

        public static bool operator >(Move left, Move right)
        {
            return left.Value < right.Value
                || (left.Value == right.Value && left.Square > right.Square);
        }
    }

    public readonly struct PlayerSettings
    {
        public bool ShowHelpers { get; }
        public bool TestMode { get; }

        public PlayerSettings(bool showHelpers, bool testMode)
        {
            ShowHelpers = showHelpers;
            TestMode = testMode;
        }
    }

    public readonly struct Settings
    {
        public int BoardSize { get; }
        public bool AutoplayMode { get; }
        public bool UseDefaults { get; }
        public bool ShowHelpers { get; }
        public bool ShowLog { get; }
        public bool TestMode { get; }

        public Settings(
            int boardSize,
            bool autoplayMode,
            bool useDefaultOptions,
            bool showHelpers,
            bool showLog,
            bool testMode
        )
        {
            BoardSize = boardSize;
            AutoplayMode = autoplayMode;
            UseDefaults = useDefaultOptions;
            ShowHelpers = showHelpers;
            ShowLog = showLog;
            TestMode = testMode;
        }

        public PlayerSettings ToPlayerSettings()
        {
            return new PlayerSettings(ShowHelpers, TestMode);
        }
    }

    public static class Extensions
    {
        public static System.Drawing.Color DiskColor(this Color disk)
        {
            if (disk == Color.Empty)
            {
                return System.Drawing.Color.White;
            }
            return disk == Color.White ? System.Drawing.Color.Cyan : System.Drawing.Color.Magenta;
        }

        public static Color Opponent(this Color disk)
        {
            if (disk == Color.Empty)
            {
                return Color.Empty;
            }
            return disk == Color.White ? Color.Black : Color.White;
        }

        public static string Name(this Color disk)
        {
            return ConsoleVisuals.Get(disk.ToString().ToUpper(), disk.DiskColor());
        }

        public static string BoardChar(this Color disk, bool color = true)
        {
            if (disk == Color.Empty)
            {
                return "_";
            }

            string diskChar = disk == Color.White ? "W" : "B";
            if (color)
            {
                return ConsoleVisuals.Get(diskChar, disk.DiskColor());
            }
            else
            {
                return diskChar;
            }
        }
    }
}
