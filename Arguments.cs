using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Arguments
    {
        List<object> positioned = new List<object>();
        Dictionary<string, object> named = new Dictionary<string, object>();

        public void Add(object arg) => positioned.Add(arg);
        public void Add(string key, object value) => named[key] = value;

        public object this[string key]
        {
            get
            {
                object res;
                if (named.TryGetValue(key, out res))
                    return res;
                else
                    return null;
            }
        }

        public object this[int index] => positioned.ElementAtOrDefault(index);
    }
}
