
using System;
using System.Threading;
using System.Collections.Generic;
using OpenMetaverse;

namespace spider
{
	class MainClass
	{
	   
	    //public static GridClient client;

        public static Database db;
        public static GridConn conn;
        public static ObjectPropTracker ObjTrack;
        public static NameTracker NameTrack;

	    static void Main()
	    {

		while(true)
		{
				
		db = new Database();
        db.OpenDatabase();
        LoginParams login = db.getlogin("Agni");

        if (login == null)
            goto exitloop;

        conn = new GridConn(login);

        if (conn.client.Network.LoginStatusCode==LoginStatus.Success)
        {
            Console.WriteLine("We are logged in ok, proceed to scrape");
        }
        else
        {
            System.Threading.Thread.Sleep(1000*60*5);
            Console.WriteLine("Login failed, we should log this and move on");
            db.CloseDatabase();
            return;
        }

        ObjTrack = new ObjectPropTracker(conn.client);
        NameTrack = new NameTracker(conn.client);
        Scraper scrape = new Scraper(conn.client);
        conn.Logout();

    exitloop:

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        Dictionary<string, string> conditions = new Dictionary<string, string>();

        MainClass.db.ExecuteSQL("UPDATE Logins SET LockID='0' WHERE LockID='" + db.myid.ToString() + "';", null, null);
        MainClass.db.ExecuteSQL("UPDATE Region SET LockID='0' WHERE LockID='" + db.myid.ToString() + "';", null, null);

        db.CloseDatabase();

        Console.WriteLine("We all go bye bye, backing off for 60 seconds");

        // Back off for 1 minute
        System.Threading.Thread.Sleep(60000);
		}
      
        
        
	    }

      
	    
	    
	}
}
