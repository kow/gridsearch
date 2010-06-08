
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
            while (true)
            {
                db = new Database();
                bool dbopen = db.OpenDatabase();
                if (!dbopen)
                {
                    Logger.Log("Cannot connect to database, going to sleep and trying later", Helpers.LogLevel.Error);
                }
               
                while (dbopen)
                {

                    //Find the inital grid
                    List<int> logingrids = db.getFirstRegionGrid(); //this locks a bunch of target regions, unique on grid
                    int logingrid;

                    if (logingrids.Count == 0)
                    {
                        Logger.Log("We got NO inital login grid, sleeping ", Helpers.LogLevel.Info);
                        break;
                    }

                    while (logingrids.Count != 0)
                    {
                        logingrid = logingrids[0];
                        logingrids.Remove(0);

                        Logger.Log("We got inital login grid of " + logingrid.ToString(), Helpers.LogLevel.Info);

                        // Get a free login slot for this grid

                        LoginParams login = db.getlogin(logingrid);
                        if (login == null)
                        {
                            // no free login slots for this grid
                            continue;
                        }

                        conn = new GridConn(login);

                        if (conn.client.Network.LoginStatusCode == LoginStatus.Success)
                        {
                            Logger.Log("We are logged in ok, proceed to scrape", Helpers.LogLevel.Info);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1000 * 60 * 5);
                            Logger.Log("Login failed, we should log this and move on", Helpers.LogLevel.Warning);
                            continue;
                        }

                        // If we are here we must have logged in ok, the first time we try to get a region from the database it will clear all the
                        // locks we no longer care about now anyway


                        ObjTrack = new ObjectPropTracker(conn.client);
                        NameTrack = new NameTracker(conn.client);
                        Scraper scrape = new Scraper(conn.client);   //FIX me we should be able to specify a limit to the number of regions before cycling

                        // scraper will block whist scraping
                        db.clearlocks();

                        conn.Logout();
                    }

                    db.clearlocks();
                }
                db.CloseDatabase();
                System.Threading.Thread.Sleep(60000);
            }
        }
    }

/*




		while(true)
		{
			
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
  */
}
