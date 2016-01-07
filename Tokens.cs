using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Tokens : Queue<Token>
    {
        public Tokens(string input)
        {
            StringBuilder s = new StringBuilder(input.Length);
            int start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case ' ':
                        if (s.Length != 0)
                        {
                            Enqueue(new Token(s.ToString(), start));
                            s.Clear();
                        }
                        start = i + 1;
                        break;
                    case '\\':
                        i++;
                        if (i < input.Length)
                            s.Append(input[i]);
                        break;
                    case '"':
                        for (; i < input.Length && input[i] != '"'; i++)
                        {
                            if (input[i] == '\\')
                                i++;
                            s.Append(input[i]);
                        }
                        break;
                    default:
                        s.Append(input[i]);
                        break;
                }
            }
            Enqueue(new Token(s.ToString(), start));
        }

        public void SetLevel(Level lvl)
        {
            foreach (Token token in this)
            {
                token.Level = lvl;
            }
        }

        public string Text() => Peek().Text;
    }

    public class Token
    {
        public string Text { get; }
        public int Index { get; }
        // TODO: length (for auto complete text in the middle of input)
        public Level Level { get; set; } = null;

        public Token(string text, int index)
        {
            Text = text;
            Index = index;
        }

        public override string ToString() => $"'{Text}' at {Index}";
    }
}
