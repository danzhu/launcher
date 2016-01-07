using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Launcher
{
    public class Command : CompletionUnit
    {
        public string ScriptSource { get; set; }
        public string Action { get; set; }
        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public bool Standalone => Parameters.Count == 0;
        public bool Executable => Action != null || Executed != null;
        public string Usage => string.Join(" ", Parameters.Select(p => p.Usage));

        [XmlIgnore]
        public ScriptScope Script { get; private set; }
        [XmlIgnore]
        public Page Widget { get; private set; }

        public event ScriptEventHandler Executed;
        public event ScriptEventHandler InputChanged;

        public Level Analyze(Session ses, Tokens tokens)
        {
            Level lvl = new Level(this);
            ses.Leaf = lvl;
            tokens.SetLevel(lvl);

            // Match parameters
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameter p = Parameters[i];
                p.Update(ses);

                MatchResult res = p.Analyze(ses, tokens, lvl);

                if (res == MatchResult.Matched)
                    lvl.FirstUnmatchedParam = i + 1;
                else if (res == MatchResult.Extensible)
                    lvl.FirstUnmatchedParam = i;
                else if (res == MatchResult.Partial)
                {
                    // if no tokens left, leave unmatched alone (to match previous params)
                    // else, update so that later ones would not go back before the param
                    if (tokens.Count > 0)
                        lvl.FirstUnmatchedParam = i;
                }
                else if (p.Required) // implied "Failed"
                {
                    // TODO: handle "required"
                    ses.Error(new ArgumentException($"{p} requires a valid value"));
                    break;
                }
            }

            // Raise event
            InputChanged?.Invoke(this, new ScriptEventArgs(ses, lvl.Arguments, lvl.Child));

            return lvl;
        }

        public bool Execute(Session ses, Arguments arg, Level child)
        {
            if (!Executable)
                return false;

            try
            {
                if (Action != null)
                    Process.Start(Action);

                if (Executed != null)
                {
                    ScriptEventArgs e = new ScriptEventArgs(ses, arg, child);
                    Executed?.Invoke(this, e);
                    return e.Succeeded;
                }

                return true;
            }
            catch (Exception e)
            {
                ses.Error(e);
                return false;
            }
        }

        public void InitializeScript(Session ses)
        {
            if (ScriptSource == null || Script != null)
                return;

            ses.Engine.LoadScriptEngine();

            try
            {
                Script = ses.Engine.ScriptEngine.CreateScope();
                Script.SetVariable("engine", ses.Engine);
                Script.SetVariable("command", this);
                ses.Engine.ScriptEngine.ExecuteFile(ScriptSource, Script);
            }
            catch (Exception e)
            {
                ses.Error(e);
            }
        }

        public void LoadWidget(WidgetLoader l)
        {
            if (Widget != null)
                return;

            // no need for try - method called fron script (within try)
            Widget = l();
        }

        public override string ToString() => $"Command '{Name}'";
    }

    public delegate Page WidgetLoader();

    public class ScriptEventArgs : EventArgs
    {
        public Engine Engine { get; }
        public Session Session { get; }
        public Arguments Arguments { get; }
        public Level Child { get; }
        public bool Succeeded { get; set; } = true;

        public ScriptEventArgs(Session ses, Arguments arg, Level child)
        {
            Engine = ses.Engine;
            Session = ses;
            Arguments = arg;
            Child = child;
        }
    }

    public delegate void ScriptEventHandler(object sender, ScriptEventArgs e);
}
