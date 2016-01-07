using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Launcher
{
    [XmlInclude(typeof(LaunchAction))]
    [XmlInclude(typeof(SetAction))]
    [XmlInclude(typeof(ExitAction))]
    public abstract class Action
    {
        public abstract void Execute(Engine engine);
    }

    public class LaunchAction : Action
    {
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public string Directory { get; set; }

        public override void Execute(Engine engine)
        {
            ProcessStartInfo info = new ProcessStartInfo(engine.Expand(FileName));
            //info.UseShellExecute = false;
            if (Arguments != null)
                info.Arguments = engine.Expand(Arguments);
            if (Directory != null)
                info.WorkingDirectory = engine.Expand(Directory);
            Process.Start(info);
        }
    }

    public class SetAction : Action
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override void Execute(Engine engine)
        {
            engine.Variables[engine.Expand(Name)] = engine.Expand(Value);
        }
    }

    public class ExitAction : Action
    {
        public int Code { get; set; } = 0;

        public override void Execute(Engine engine)
        {
            Application.Current.Shutdown(Code);
        }
    }
}
