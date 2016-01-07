using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Option : CompletionUnit
    {
        string name;
        public override string Name
        {
            get
            {
                return name != null ? name : (Long != null ? Long : Short);
            }

            set
            {
                name = value;
            }
        }

        public override string Display
            => (Short != null ? OptionShort + ' ' : "   ")
            + (Long != null ? OptionLong : "");

        public string Short { get; set; }
        public string Long { get; set; }
        public string OptionShort => Short == null ? null : '-' + Short;
        public string OptionLong => Long == null ? null : "--" + Long;

        public bool Is(string str) => str == OptionShort || str == OptionLong;

        public override string Complete(string input)
            => Short == null || input.StartsWith("--") ? OptionLong : OptionShort;
    }
}
