using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Launcher
{
    [XmlInclude(typeof(CommandParameter))]
    [XmlInclude(typeof(OptionParameter))]
    [XmlInclude(typeof(TextParameter))]
    public abstract class Parameter
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public bool Required { get; set; }

        public virtual string Usage => Required ? Name : $"[{Name}]";

        public abstract void Complete(Completion cmp, string input, Arguments args);

        // Match each input and return true if match > 0
        // (terminates at first no-match)
        public abstract MatchResult Analyze(Session ses, Tokens tokens, Level lvl);

        public virtual void Update(Session ses)
        {
            // default: do nothing
        }

        public static void Complete<T>(Completion cmp, IList<T> list, Func<T, int> selector)
            where T : CompletionUnit
        {
            foreach (T item in list)
            {
                if ((item.Completion.MatchLevel = selector(item)) < 0)
                    continue;
                cmp.Insert(item.Completion);
            }
        }

        // Base match algorithm
        public static int Match(string src, string input)
        {
            // For switches (no Short / no Long)
            if (src == null)
                return -1;

            // Test exact match
            if (src == input)
                return 2;

            // Test partial match
            if (src.StartsWith(input))
                return 1;

            // Test intermittent match
            while (input.Length > 0)
            {
                int i = src.IndexOf(input[0]);
                if (i < 0)
                    return -1; // No match
                src = src.Substring(i + 1);
                input = input.Substring(1);
            }
            return 0;
        }

        public override string ToString() => $"{GetType().Name} '{Name}'";
    }

    public class CommandParameter : Parameter
    {
        public List<Command> Commands { get; } = new List<Command>();

        public CommandParameter()
        {
            Name = "command";
        }

        public override void Complete(Completion cmp, string input, Arguments args)
            => Complete(cmp, Commands, c => Match(c.Name, input));

        public override MatchResult Analyze(Session ses, Tokens tokens, Level lvl)
        {
            if (tokens.Count == 0)
                return MatchResult.Failed;

            string text = tokens.Text();
            Command cmd = Commands.Find(c => c.Name == text);
            if (cmd == null)
                return MatchResult.Failed;

            tokens.Dequeue();
            lvl.Child = cmd.Analyze(ses, tokens);
            return MatchResult.Matched;
        }

        public override void Update(Session ses)
        {
            Commands.ForEach(c => c.InitializeScript(ses));
        }
    }

    public class OptionParameter : Parameter
    {
        public List<Option> Options { get; } = new List<Option>();

        public OptionParameter()
        {
            Name = "options";
        }

        public override void Complete(Completion cmp, string input, Arguments args)
            => Complete(cmp, Options, o => Match(o, input, args));

        public override MatchResult Analyze(Session ses, Tokens tokens, Level lvl)
        {
            if (tokens.Count == 0)
                return MatchResult.Failed;

            Option op;
            while (tokens.Count > 0 && (op = Options.Find(o => o.Is(tokens.Text()))) != null)
            {
                tokens.Dequeue();
                lvl.Arguments.Add(op.Name, true);
            }
            return MatchResult.Extensible;
        }

        private static int Match(Option op, string input, Arguments args)
        {
            // Remove duplicates
            if (true.Equals(args[op.Name]))
                return -1;

            // Empty string matches all at level 0 (except duplicates)
            if (input.Length == 0)
                return 0;

            if (input[0] != '-')
                return Math.Max(Match(op.Long, input), Match(op.Short, input));

            if (!input.StartsWith("--") && op.Short != null)
                return Match(op.Short, input.Substring(1));
            else
                return Match(op.Long, input.TrimStart('-')); // no substring since length might be 1
        }
    }

    public class TextParameter : Parameter
    {
        public override void Complete(Completion cmp, string input, Arguments args)
            => cmp.AutoComplete = false;

        public override MatchResult Analyze(Session ses, Tokens tokens, Level lvl)
        {
            if (tokens.Count == 0)
                return MatchResult.Failed;

            lvl.Arguments.Add(tokens.Dequeue().Text);
            // TODO: text removes previous auto completion
            return MatchResult.Partial;
        }
    }

    public enum MatchResult
    {
        Failed,
        Partial,
        Matched,
        Extensible
    }
}
