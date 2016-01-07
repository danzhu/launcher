using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Launcher
{
    // TODO: isolate CompletionItem and Widget from Command and bind to Session
    public class Session
    {
        public Engine Engine { get; }
        public Token Input { get; set; }
        public Level Root { get; set; }
        public Level Leaf { get; set; }
        public Completion Completion { get; set; }

        Command scope;
        public Command Scope
        {
            get { return scope; }
            set
            {
                scope = value;
                ScopeChanged?.Invoke(this, value);
            }
        }

        Page widget;
        public Page Widget
        {
            get { return widget; }
            set
            {
                widget = value;
                WidgetChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<Page> WidgetChanged;
        public event EventHandler<Level> CommandExecuted;
        public event EventHandler<Exception> ExceptionThrown;
        public event EventHandler<Command> ScopeChanged;

        public Session(Engine eng)
        {
            Engine = eng;
            Scope = eng.Main;
        }

        public void ExecuteCommand(Engine eng, Level lvl)
        {
            // Execute or change scope
            if (!lvl.Command.Executable)
            {
                Scope = lvl.Command;
            }
            else if (lvl.Execute(this))
            {
                // Only raise event and exit if command is actually executed
                CommandExecuted?.Invoke(this, lvl);

                // TODO: separate config & event
                if (eng.AutoExit)
                    Application.Current.Shutdown();
            }
        }

        public void Error(Exception e)
        {
            ExceptionThrown?.Invoke(this, e);
        }
    }
}
