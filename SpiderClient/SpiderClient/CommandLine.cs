using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;

namespace spider
{
    class CommandLine
    {
        Dictionary<string, string> optionargs = new Dictionary<string, string>();
		List<string> required_parameters = new List<string>();

		public void addRequiredCLP(string name)
		{
			if(!required_parameters.Contains(name))
			{
				required_parameters.Add(name);
			}
		}
		
        public bool parsecommandline()
        {
            string[] allargs = Environment.GetCommandLineArgs();
			List<string> required_parameters_notfound = required_parameters;
			
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
					
					if(required_parameters_notfound.Contains(thearg))
					{
						required_parameters_notfound.Remove(thearg);	
					}
                }
            }
			
			if(required_parameters_notfound.Count>0)
			{
				
				foreach(string missing in required_parameters_notfound)
				{
					Logger.Log("Required command line parameter not found : "+missing,Helpers.LogLevel.Error);
				}
				return false;
			}
			
			return true;
        }
		
		public string getopt(string name)
		{
			string val;
			
			if(optionargs.TryGetValue(name,out val))
			{
				return val;	
			}
			else
			{
				return null;	
			}
			
			
		}
    }
}
