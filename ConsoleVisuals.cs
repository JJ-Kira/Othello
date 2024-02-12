using Pastel;
using System.Drawing;

namespace Othello
{
    public static class ConsoleVisuals
    {
        public static string Get<T>(T text, System.Drawing.Color color)
        {
            return $"{text}".Pastel(color);
        }

        public static void Write<T>(T text, System.Drawing.Color color)
        {
            Console.Write($"{text}".Pastel(color));
        }

        public static void WriteLine<T>(T text, System.Drawing.Color color)
        {
            Console.WriteLine($"{text}".Pastel(color));
        }

        /// <summary>
        /// Prints error message with red colour.
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            var (indent, text) = SplitLeadingWhitespace(message);
            Console.WriteLine($"{indent}Error: {message}".Pastel(System.Drawing.Color.Red));
        }

        /// <summary>
        /// Prints warning message with yellow colour.
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            var (indent, text) = SplitLeadingWhitespace(message);
            Console.WriteLine($"{indent}Warning: {text}".Pastel(System.Drawing.Color.Yellow));
        }

        /// <summary>
        /// Splits a string into the leading whitespace and the rest of the string.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static (string, string) SplitLeadingWhitespace(string message)
        {
            // Find the index of the first non-whitespace character.
            int indentSize = 0;
            foreach (char c in message)
            {
                if (char.IsWhiteSpace(c))
                    indentSize++;
                else
                    break;
            }

            return (message[..indentSize], message[indentSize..]);
        }
    }
}
