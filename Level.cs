using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Level
    {
        public Command Command { get; }
        public Arguments Arguments { get; } = new Arguments();
        public Level Child { get; set; }
        public int FirstUnmatchedParam { get; set; } = 0;

        public Level(Command cmd)
        {
            Command = cmd;
        }

        public bool Execute(Session ses)
        {
            return Command.Execute(ses, Arguments, Child);
        }

        public override string ToString() => $"Level '{Command.Name}'";
    }
}
