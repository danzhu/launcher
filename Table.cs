using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Table
    {
        public List<string> Headers { get; } = new List<string>();
        public List<string[]> Rows { get; } = new List<string[]>();

        public void AddHeader(string name, string def = null)
        {
            Headers.Add(name);
            for (int i = 0; i < Rows.Count; i++)
            {
                int length = Rows[i].Length;
                string[] row = new string[length + 1];
                Rows[i].CopyTo(row, 0);
                row[length] = def;
                Rows[i] = row;
            }
        }
    }
}
