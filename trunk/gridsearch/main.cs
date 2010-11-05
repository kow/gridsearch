
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
				
				CommandLine cl= new CommandLine();
				cl.addRequiredCLP("host");
				cl.addRequiredCLP("user");
				cl.addRequiredCLP("database");

				if(!cl.parsecommandline())
				{
					return;	
				}
				
                db = new Database();
                bool dbopen = db.OpenDatabase(cl.getopt("host"),cl.getopt("user"),cl.getopt("database"),cl.getopt("port"),cl.getopt("password"));
                if (!dbopen)
                {
                    Logger.Log("Cannot connect to database, going to sleep and trying later", Helpers.LogLevel.Error);
                }
               
                while (dbopen)
                {

                    if (cl.getopt("region_list")!=null && cl.getopt("for_grid")!=null)
                    {
                        string region_list = cl.getopt("region_list");
                        string grid = cl.getopt("for_grid");
                        long key = db.getgridkey(grid);

                        System.IO.StreamReader SR;
                        string line;
                        SR = System.IO.File.OpenText(region_list);


                        line = SR.ReadLine();
                        while (line !=null)
                        {
                            // Format is name/tregionX/tRegionY
                            try
                            {
                                string[] parts = line.Split('\t');

                                string region_name = parts[0];

                                int X = int.Parse(parts[1]);
                                int Y = int.Parse(parts[2]);

                                float localX, localY;
                                ulong regionhandle = Helpers.GlobalPosToRegionHandle((X*256) + 128, (Y*256) + 128,out localX, out localY);

                                Dictionary<string, string> parameters = new Dictionary<string, string>();
                                Dictionary<string, string> conditions = new Dictionary<string, string>();
                                parameters.Add("Grid", key.ToString());
                                parameters.Add("Handle", regionhandle.ToString());
                                parameters.Add("Name", region_name);
                                MainClass.db.genericInsertIgnore("Region", parameters);
                            }
                            catch
                            {
                                Logger.Log("Failed to parse " + line, Helpers.LogLevel.Warning);

                            }
                            line = SR.ReadLine();
                        }

                        SR.Close();
			Logger.Log("Finished importing region list to "+grid,Helpers.LogLevel.Info);
                        return;
                    }

                    Logger.Log("Starting a new master scrape loop", Helpers.LogLevel.Info);

                    //Find the inital grid
                    List<int> logingrids = db.getFirstRegionGrid(); //this locks a bunch of target regions, unique on grid;
                    
                    int logingrid;

                    if (logingrids.Count == 0)
                    {
                        Logger.Log("We got NO inital login grid, sleeping ", Helpers.LogLevel.Info);
                        break;
                    }

                    while (logingrids.Count != 0)
                    {
                        Logger.Log("Current login list has "+logingrids.Count.ToString()+" entries", Helpers.LogLevel.Info);

                        logingrid = logingrids[0];
                        logingrids.Remove(logingrid);

                        Logger.Log("Trying login on grid # " + logingrid.ToString(), Helpers.LogLevel.Info);

                        // Get a free login slot for this grid
                        
                        LoginParams login = db.getlogin(logingrid);
                        if (login == null)
                        {
                            // no free login slots for this grid
                            Logger.Log("No free login slots on grid # " + logingrid.ToString(), Helpers.LogLevel.Info);
                            continue;
                        }

                        long handle;
                        String name = db.getNextRegionForGrid(out handle);
                        
                        //Chcek if there are valid regions
                        if (db.gridhasregions==true && db.regionsremaining == false)
                        {
                            // Well thats a bad start no regions on this grid worth checking currently ???
                            Logger.Log("Grid has no regions valid for spidering right now, why are we here? skipping login ", Helpers.LogLevel.Warning);
                            continue;
                        }

                        if(db.gridhasregions==false)
                        {
                            Logger.Log("This dam well better be a new grid with no regions yet in the db", Helpers.LogLevel.Warning);
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
                    Logger.Log("Post logout, backing off for 60 seconds", Helpers.LogLevel.Info);
                    System.Threading.Thread.Sleep(60000);
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
