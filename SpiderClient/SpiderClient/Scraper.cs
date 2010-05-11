using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using System.Threading;

namespace spider
{
    class Scraper
    {
		
		GridClient client;
        Timer timer;

        public Scraper(GridClient theclient)
        {
			
			client=theclient;
		    TimerCallback timerDelegate = new TimerCallback(TimerProc);
			timer = new Timer(timerDelegate, null, 1000, 1000);

            Console.WriteLine("Starting scrape");
            scraperlogic();

        }

        public void terminate()
        {
           
        }

        public string getNextRegion()
        {
            string region = "";
            Int64 handle;
            region = MainClass.db.getNextRegionForGrid(out handle);
            Console.WriteLine("Get Next Region returned \"" + region+"\"");

            if (MainClass.db.gridhasregions == false)
            {
                Console.WriteLine("The grid has no registered regions, just logging in and learning from there");
                //just insert current region then and proceed from there
                Simulator sim = MainClass.conn.getRegion();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("Grid", MainClass.db.gridKey.ToString());
                parameters.Add("Handle", sim.Handle.ToString());
                parameters.Add("Name", sim.Name);
                parameters.Add("Owner", sim.SimOwner.ToString());

                MainClass.db.genericReplaceInto("Region", parameters, false);
                region = MainClass.conn.getRegion().Name;
                MainClass.db.regionsremaining = true;
                return client.Network.CurrentSim.Name;
            }
            
            if(handle==0)
            {
                Console.WriteLine("Moving to a newly entered region");
              
                
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    Dictionary<string, string> conditions = new Dictionary<string, string>();

                    conditions.Add("Grid", MainClass.db.gridKey.ToString());
                    conditions.Add("Name", MainClass.conn.client.Network.CurrentSim.Name);

                    parameters.Add("Handle", MainClass.conn.client.Network.CurrentSim.Handle.ToString());
                 
                    MainClass.db.genericUpdate("Region", parameters, conditions);

            }
			
			if (region != "")
            {
				return region;                
            }
			
			return "";


        }

        private static void TimerProc(object state)
        {
            // The state object is the Timer object.

            MainClass.conn.rotate();

        }

		bool doscrapeloop(string simname,Vector3 position)
		{
			
			TimeSpan wait=new TimeSpan(0);	
			DateTime start=DateTime.Now;
			
			if(!client.Self.Teleport(simname,position))
			{
				Console.WriteLine("Teleport to "+simname+" failed");
				
				//Sleep 7 seconds to cool off
				System.Threading.Thread.Sleep(7000);
				return false;
			}
			
			// Ok we are in position

            MainClass.conn.client.Parcels.RequestAllSimParcels(MainClass.conn.client.Network.CurrentSim);
			
			while(true)
			{
				wait=DateTime.Now- start;
				if(MainClass.conn.connected==false)
				{
					Console.WriteLine("Breaking scrape loop disconnected");
					return false;
					break;
				}
			
				//Make sure we are all completed and have waited at least 1 mins, 5 mins and we are bored though
				if((MainClass.ObjTrack.complete() && MainClass.NameTrack.complete() && wait.Seconds>15) || wait.Minutes>5 )
				{
					Console.WriteLine("Object track, and wait time satisified breaking loop");
					return true;
					break;	
				}
				
				System.Threading.Thread.Sleep(100);
			}
			
			return true;
			
		}
		
		void mark_region_bad(string region)
		{
			
			 Dictionary<string, string> parameters = new Dictionary<string, string>();
             Dictionary<string, string> conditions = new Dictionary<string, string>();

             conditions.Add("Grid", MainClass.db.gridKey.ToString());
             conditions.Add("Name", region);
             parameters.Add("Status", (-1).ToString());
             MainClass.db.genericUpdate("Region", parameters, conditions);
		}
		
		
        void scraperlogic()
        {
            //Get a region from the top of the stack for this grid

            while (MainClass.db.regionsremaining)
            {

				// Are we still connected
                

				string region=getNextRegion();
				
				if(region=="")
				{
					Console.WriteLine("Got bad region from database");
					continue;
				}
				
				Console.WriteLine("Starting scrape loop for region "+region);
				
				bool anyok=false;
				
				anyok |= doscrapeloop(region,new OpenMetaverse.Vector3(128, 128, 25));
				
				if (MainClass.conn.connected == false)
                    break;
				
                /*
				anyok |= doscrapeloop(region,new OpenMetaverse.Vector3(340, 340, 0));
				
				if (MainClass.conn.connected == false)
                    break;
				
				anyok |= doscrapeloop(region,new OpenMetaverse.Vector3(170, 170, 0));
				
				if (MainClass.conn.connected == false)
                    break;
				
				anyok |= doscrapeloop(region,new OpenMetaverse.Vector3(170, 340, 0));
				
                 */
                 
				if (MainClass.conn.connected == false)
                    break;
				
				if(anyok==false)
				{
					mark_region_bad(region);
					continue;
				}
				
                Console.WriteLine("Scan complete waiting parcel update before saving");

				DateTime start=DateTime.Now;
				TimeSpan timeout=new TimeSpan(0);
				
                while (!MainClass.conn.client.Network.CurrentSim.IsParcelMapFull() && !MainClass.ObjTrack.complete() && timeout.Minutes<2  )                
				{
                    System.Threading.Thread.Sleep(100);
					timeout=DateTime.Now-start;
                }

                Console.WriteLine("All Properties recieved, saving data");

                MainClass.ObjTrack.saveallprims();

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Dictionary<string, string> conditions = new Dictionary<string, string>();

                conditions.Add("Grid", MainClass.db.gridKey.ToString());
                conditions.Add("Handle", MainClass.conn.client.Network.CurrentSim.Handle.ToString());
                parameters.Add("Owner", MainClass.db.compressUUID(MainClass.conn.client.Network.CurrentSim.SimOwner));
                parameters.Add("Status", (0).ToString());

                MainClass.db.genericUpdate("Region", parameters, conditions);

				//Fix me i should be based on teleport and sim name tracking
                MainClass.conn.gotallparcels = false;
             
				//Fix me i should be based on teleport
                MainClass.ObjTrack.flush_for_new_sim();

            }
        }
    }
}
