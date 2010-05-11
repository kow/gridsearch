
using System;
using OpenMetaverse;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace spider
{
	class MainClass
	{
	    public static readonly Dictionary<string,string> optionargs = new Dictionary<string,string>();
	    public static GridClient client;

	    static void Main()
	    {		
		string[] allargs=Environment.GetCommandLineArgs();
				
		int x=0;
		foreach( string args in allargs )
		{
		    x++;	    
		    if( args.Substring(0,2) == "--" )
		    {
			string thearg=args.Substring(2,args.Length-2);
			if(x<allargs.Length)
			{
			    if(allargs[x].Substring(0,2) == "--")
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
		
		client=new GridClient();
		
		
						
	    }
	    
	    static int savetodatabase()
	    {
	    
		// Database test
		
		string connStr = "server=192.168.0.3;user=root;database=spider;port=3306;password=louise42";
		MySqlConnection conn = new MySqlConnection(connStr);
		try
		{
		    Console.WriteLine("Connecting to MySQL..");
		    conn.Open();
		}
		catch (Exception e)
		{
		    Console.WriteLine(e.ToString());
		}
		
		conn.Close();	
		
		Console.WriteLine("Done");
		
		// End of database
	    
	    
	    }
	    
	    
	}
}
