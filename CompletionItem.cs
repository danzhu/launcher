using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Launcher
{
    public class CompletionItem : ListBoxItem
    {
        public CompletionUnit Unit { get; }
        public int MatchLevel { get; set; }

        public CompletionItem(CompletionUnit u)
        {
            Unit = u;
            Content = u.Display;
        }

        public override string ToString() => $"'{Unit.Name}' at level {MatchLevel}";
    }
}
