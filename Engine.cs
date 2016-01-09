using IronPython.Hosting;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Launcher
{
    public class Engine
    {
        public bool AutoComplete { get; set; } = true;
        public bool AutoExit { get; set; } = true;
        public bool AutoLaunch { get; set; } = false;
        // TODO: use this path
        public string CommandsPath { get; set; } = "commands";

        public Command Main { get; set; }

        [XmlIgnore]
        public ScriptEngine ScriptEngine { get; private set; }

        [XmlIgnore]
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

        private static XmlSerializer serializer = new XmlSerializer(typeof(Engine));

        public Engine() { }

        public void LoadScriptEngine()
        {
            if (ScriptEngine != null)
                return;
            ScriptEngine = Python.CreateEngine();
            ScriptRuntime runtime = ScriptEngine.Runtime;
            runtime.LoadAssembly(typeof(Wpf).Assembly);
            runtime.LoadAssembly(typeof(Page).Assembly);
        }

        public void Save(string file)
        {
            using (FileStream fs = File.OpenWrite(file))
                serializer.Serialize(fs, this);
        }

        public void Analyze(Session ses, string input)
        {
            Tokens tokens = new Tokens(input);

            ses.Input = tokens.Last();
            ses.Root = ses.Scope.Analyze(ses, tokens);

            if (tokens.Count > 0)
                ses.Input = tokens.First();

            // If script is not loaded, it's time to load now
            ses.Leaf.Command.InitializeScript(ses);

            // Possibly empty - to clear widget when out of scope
            ses.Widget = ses.Leaf.Command.Widget;

            // Auto complete
            ses.Completion = new Completion();
            Level lvl = ses.Input.Level;
            List<Parameter> param = lvl.Command.Parameters;
            for (int i = lvl.FirstUnmatchedParam; i < param.Count; i++)
            {
                param[i].Complete(ses.Completion, ses.Input.Text, ses.Leaf.Arguments);
                // no auto completion after required parameter
                if (param[i].Required)
                    break;
            }

            // Auto launch
            if (AutoLaunch && ses.Completion.Completions.Count == 1)
            {
                Command cmd = ses.Completion.Completions[0].Unit as Command;
                if (cmd?.Standalone == true)
                    ses.ExecuteCommand(this, new Level(cmd));
            }
        }

        public string Expand(string str)
        {
            // TODO: Expand variables
            return str;
        }

        public static Engine Load(string file)
        {
            try
            {
                using (FileStream fs = File.OpenRead(file))
                    return (Engine)serializer.Deserialize(fs);
            }
            catch (FileNotFoundException)
            {
                Engine engine = new Engine();
                engine.Save(file);
                return engine;
            }
        }
    }

    public class Completion
    {
        public bool AutoComplete { get; set; } = true;
        public List<CompletionItem> Completions { get; } = new List<CompletionItem>();

        public void Insert(CompletionItem m)
        {
            for (int i = Completions.Count - 1; i >= 0; i--)
            {
                if (Completions[i].MatchLevel < m.MatchLevel)
                    continue;
                Completions.Insert(i + 1, m);
                return;
            }

            Completions.Insert(0, m);
        }
    }
}
