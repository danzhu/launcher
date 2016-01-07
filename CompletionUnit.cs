using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class CompletionUnit
    {
        public virtual string Name { get; set; }
        public string Info { get; set; }
        public virtual string Display => Name;

        // Generate completion when required (minimize startup time)
        CompletionItem completion;
        public CompletionItem Completion
            => completion == null ? completion = new CompletionItem(this) : completion;

        public virtual string Complete(string input) => Name;
    }
}
