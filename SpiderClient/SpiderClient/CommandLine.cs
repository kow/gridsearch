using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spider
{
    class CommandLine
    {
        public readonly Dictionary<string, string> optionargs = new Dictionary<string, string>();

        public void parsecommandline()
        {
            string[] allargs = Environment.GetCommandLineArgs();

            int x = 0;
            foreach (string args in allargs)
            {
                x++;
                if (args.Substring(0, 2) == "--")
                {
                    string thearg = args.Substring(2, args.Length - 2);
                    if (x < allargs.Length)
                    {
                        if (allargs[x].Substring(0, 2) == "--")
                        {
                            optionargs[thearg] = "TRUE";
                        }
                        else
                        {
                            optionargs[thearg] = allargs[x];
                        }
                    }
                    else
                    {
                        optionargs[thearg] = "TRUE";
                    }
                }
            }

        }
    }
}
