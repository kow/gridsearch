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
       
        public Scraper(GridClient theclient)
        {		
			client=theclient;
		    
            Console.WriteLine("Starting scrape");
            scraperlogic();
        }

        public void terminate()
        {
           
        }

        public string getNextRegion(out ulong regionhandle)
        {
            string region = "";
            Int64 handle;
			regionhandle=0;
			
            region = MainClass.db.getNextRegionForGrid(out handle);
            Logger.Log("Get Next Region returned \"" + region + "\"", Helpers.LogLevel.Info);

            if (MainClass.db.gridhasregions == false)
            {
                Logger.Log("The grid has no registered regions, just logging in and learning from there", Helpers.LogLevel.Info);

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
				regionhandle=(ulong)handle;
                return client.Network.CurrentSim.Name;
            }

            if (MainClass.db.gridhasregions == true && MainClass.db.regionsremaining == false)
            {
                //There are no regions left to process on this grid, why are we here?
                return "";
            }
            
            if(handle==0)
            {
                    Logger.Log("Moving to a newly entered region", Helpers.LogLevel.Info);
                
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    Dictionary<string, string> conditions = new Dictionary<string, string>();

                    conditions.Add("Grid", MainClass.db.gridKey.ToString());
                    conditions.Add("Name", MainClass.conn.client.Network.CurrentSim.Name);

                    parameters.Add("Handle", MainClass.conn.client.Network.CurrentSim.Handle.ToString());
                 
                    MainClass.db.genericUpdate("Region", parameters, conditions);

            }
			
			regionhandle=(ulong)handle;
			
			if (region != "")
            {
				return region;                
            }
			
			return "";


        }

		bool doscrapeloop(string simname,ulong handle, Vector3 position)
		{
			
			TimeSpan wait=new TimeSpan(0);	
			DateTime start=DateTime.Now;
			
			bool result;
			
            Logger.Log(String.Format("Trying to teleport to {0} {1}", simname, handle), Helpers.LogLevel.Info);

			if(handle==0)
			{
                Logger.Log("Handle is 0 using simname", Helpers.LogLevel.Info);
				result=client.Self.Teleport(simname,position);
			}
			else
			{
				result=client.Self.Teleport(handle,position);
			}
			
            // did TP fail?
            // Are we where we should be, or did we get redirected?
			if(!result || handle != client.Network.CurrentSim.Handle)
			{
                                Logger.Log("Teleport to " + simname + " failed", Helpers.LogLevel.Warning);
				MainClass.NameTrack.active=false;
				MainClass.ObjTrack.active=false;
			
				//Sleep 7 seconds to cool off
				System.Threading.Thread.Sleep(15000);
				return false;
			}

			// Ok we are in position

            MainClass.conn.client.Parcels.RequestAllSimParcels(MainClass.conn.client.Network.CurrentSim);
			
			while(true)
			{
				wait=DateTime.Now - start;
				if(MainClass.conn.connected==false)
				{
					Console.WriteLine();
                    Logger.Log("Breaking scrape loop disconnected", Helpers.LogLevel.Warning);

					return false;
					break;
				}

                //Logger.Log("dowork Objects : " + MainClass.ObjTrack.complete().ToString() + MainClass.ObjTrack.requested_props.Count.ToString() + "/" + MainClass.ObjTrack.requested_propsfamily.Count.ToString() + "/" + MainClass.ObjTrack.intereset_list.Count.ToString() + " Names :" + MainClass.NameTrack.complete().ToString() + " : " + MainClass.NameTrack.agent_names_requested.Count.ToString() + " time :" + wait.Minutes.ToString() + ":" + wait.Seconds.ToString(), Helpers.LogLevel.Warning);

				//Make sure we are all completed and have waited at least 1 mins, 5 mins and we are bored though
				if((MainClass.ObjTrack.complete() && MainClass.NameTrack.complete() && (wait.Minutes >=1 ||wait.Seconds >= 20 )&& MainClass.conn.gotallparcels==true) || wait.Minutes>=2 )
				{
                    Logger.Log("Object track, and wait time satisified breaking loop",Helpers.LogLevel.Info);
					return true;
					break;	
				}
				
				System.Threading.Thread.Sleep(250);
                MainClass.conn.rotate();
			}
			
			return true;
			
		}

        void mark_region_bad(ulong handle)
		{


            Logger.Log("Trying to mark region bad " + handle.ToString(), Helpers.LogLevel.Warning);

			 Dictionary<string, string> parameters = new Dictionary<string, string>();
             Dictionary<string, string> conditions = new Dictionary<string, string>();

             conditions.Add("Grid", MainClass.db.gridKey.ToString());
             conditions.Add("Handle", handle.ToString());
             parameters.Add("Status", (-1).ToString());
             MainClass.db.genericUpdate("Region", parameters, conditions);
		}
		
		
        void scraperlogic()
        {
            //Get a region from the top of the stack for this grid

            while (MainClass.db.regionsremaining && MainClass.conn.connected)
            {

				// Are we still connected
                
				ulong handle;
				bool lastbad=false;
				string region=getNextRegion(out handle);

                if (MainClass.db.regionsremaining == false)
                {
                    // No regions left to process
                    Logger.Log("There are no regions left to process, going to logout", Helpers.LogLevel.Info);
                    break;
                }

				if(region=="" && handle==0)
				{
                    Logger.Log("Got bad region from database", Helpers.LogLevel.Error);
					continue;
				}

                Logger.Log("Starting scrape loop for region " + region, Helpers.LogLevel.Info);
				
				bool anyok=false;
				
				MainClass.ObjTrack.flush_for_new_sim();
			        MainClass.conn.gotallparcels = false;
			        if(lastbad==false)
			        {
			            MainClass.NameTrack.active=true;
			            MainClass.ObjTrack.active=true;
                  		    MainClass.conn.mapwalk();
			        }
	
				anyok |= doscrapeloop(region,handle,new OpenMetaverse.Vector3(10,10, 25));
				
				if (MainClass.conn.connected == false)
                    break;
				
				anyok |= doscrapeloop(region,handle,new OpenMetaverse.Vector3(10, 245, 25));
				
				if (MainClass.conn.connected == false)
                    break;
				
				anyok |= doscrapeloop(region,handle,new OpenMetaverse.Vector3(245, 245, 25));
				
				if (MainClass.conn.connected == false)
                    break;
				
				anyok |= doscrapeloop(region,handle,new OpenMetaverse.Vector3(245, 10, 25));
				
                
				if (MainClass.conn.connected == false)
                    break;
				
				if(anyok==false)
				{
                                        mark_region_bad(handle);
					lastbad=true;
                                        continue;
				}
				lastbad=false;
				
                Console.WriteLine();
                Logger.Log("Scan complete waiting parcel update before saving", Helpers.LogLevel.Info);

				DateTime start=DateTime.Now;
				TimeSpan timeout=new TimeSpan(0);
				
                while (!MainClass.conn.client.Network.CurrentSim.IsParcelMapFull() && !MainClass.ObjTrack.complete() && timeout.Minutes<2  )                
				{
                    System.Threading.Thread.Sleep(100);
					timeout=DateTime.Now-start;
                }

                Logger.Log("All Properties recieved, saving data", Helpers.LogLevel.Info);

				MainClass.NameTrack.active=false;
				MainClass.ObjTrack.active=false;
                MainClass.ObjTrack.saveallprims();
				MainClass.NameTrack.savenamestodb();

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Dictionary<string, string> conditions = new Dictionary<string, string>();

                conditions.Add("Grid", MainClass.db.gridKey.ToString());
                conditions.Add("Handle", MainClass.conn.client.Network.CurrentSim.Handle.ToString());
                parameters.Add("Name", MainClass.conn.client.Network.CurrentSim.Name);
                parameters.Add("Owner", MainClass.db.compressUUID(MainClass.conn.client.Network.CurrentSim.SimOwner));
                parameters.Add("Status", (0).ToString());

                MainClass.db.genericUpdate("Region", parameters, conditions);

                parameters.Clear();
                conditions.Clear();

                conditions.Add("LockID",MainClass.db.myid.ToString());
                MainClass.db.genericUpdate("Logins", parameters, conditions);
            }
        }
    }
}
