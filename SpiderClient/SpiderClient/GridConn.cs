using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;
using System.Threading;

namespace spider
{
    class GridConn
    {
        public GridClient client;
        double angle;

        public bool connected=false;

      
       
        List<UUID> prop_request;
       

        public List<UUID> agent_properties_wait;
        public Dictionary<UUID, DateTime> agent_properties_queue;
        public List<UUID> agent_properties_recieved;

       
        

        public bool gotallparcels = false;

        public GridConn(LoginParams data)
        {
            client = new GridClient();
            prop_request = new List<UUID>();

          

            agent_properties_recieved = new List<UUID>();
            agent_properties_queue = new Dictionary<UUID, DateTime>();
            agent_properties_wait = new List<UUID>();

            LoginParams login = new LoginParams();
            login = client.Network.DefaultLoginParams(data.FirstName,data.LastName,data.Password,"GridSpider","1.0");
            login.URI = data.URI;
            login.Channel = "Test Grid Spider";
            login.Version = "1.0";

            client.Settings.OBJECT_TRACKING = true;
            client.Settings.PARCEL_TRACKING = true;
            client.Settings.ALWAYS_REQUEST_OBJECTS = false;
            client.Settings.SEND_AGENT_UPDATES = true;
            client.Settings.MULTIPLE_SIMS = true;
         

            client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(Network_SimConnected);
            client.Network.EventQueueRunning += new EventHandler<EventQueueRunningEventArgs>(Network_EventQueueRunning);
            client.Parcels.SimParcelsDownloaded += new EventHandler<SimParcelsDownloadedEventArgs>(Parcels_SimParcelsDownloaded);
            client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
			
			client.Network.LoggedOut +=	new EventHandler<LoggedOutEventArgs>(Network_LoggedOut);
			client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
			
            client.Network.Login(login);

            client.Self.Movement.Camera.Far = 512;
            client.Self.Movement.SendUpdate(true);

            if (client.Network.LoginStatusCode == LoginStatus.Success)
            {
                Console.WriteLine("Login procedure completed");
				connected=true;
                
				//Logger.Log("message",Helpers.LogLevel.Info);
				
            }
        }
		
		void Network_SimDisconnected(object sender, SimDisconnectedEventArgs e)
		{
			Console.WriteLine("Sim disconnected from "+e.Simulator.Name+" Reason "+e.Reason);
			if(client.Network.CurrentSim==e.Simulator)
			{

				Console.WriteLine("*** BONED WE HAVE BEEN BOOTED ***");

                if (connected == true)
                {
					connected = false;
                    client.Network.Logout(); //force logout to clean up libomv
                }
				
            }
			
		}
		
		void Network_LoggedOut(object sender, LoggedOutEventArgs e)
		{
			Console.WriteLine("***** LOGOUT RECIEVED ITS ALL OVER ********");
			connected=false;
		}

        void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            if (e.Simulator.Name != client.Network.CurrentSim.Name)
            {
				
			    if(e.Simulator.Name=="")
					return;
			
                Console.WriteLine("New sim connection from " + e.Simulator.Name);

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("Grid", MainClass.db.gridKey.ToString());
                parameters.Add("Handle", e.Simulator.Handle.ToString());
                parameters.Add("Name", e.Simulator.Name);
                parameters.Add("Owner", MainClass.db.compressUUID(e.Simulator.SimOwner));


                ThreadPool.QueueUserWorkItem(sync =>
                {
                    MainClass.db.genericInsertIgnore("Region", parameters);
                });
            }
        }

       


        void Self_TeleportProgress(object sender, TeleportEventArgs e)
        {
            
	           Console.WriteLine("TP Update --> "+e.Message.ToString()+" : "+e.Status.ToString());
        }


        void Parcels_SimParcelsDownloaded(object sender, SimParcelsDownloadedEventArgs e)
        {
            if (gotallparcels == true)
                return;

            gotallparcels = true;

                ThreadPool.QueueUserWorkItem(sync =>
                {

                    Console.WriteLine("** GOT ALL SIM PARCELS **");
        
                    e.Parcels.ForEach(delegate(KeyValuePair<int, Parcel> kvp)
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add("Grid", MainClass.db.gridKey.ToString());
                        parameters.Add("Region", e.Simulator.Handle.ToString());
                        parameters.Add("ParcelID", kvp.Key.ToString());
                        parameters.Add("Description", kvp.Value.Desc);
                        parameters.Add("Name", kvp.Value.Name);
                        parameters.Add("Size", kvp.Value.Area.ToString());
                        parameters.Add("Dwell", kvp.Value.Dwell.ToString());
                        parameters.Add("Owner", MainClass.db.compressUUID(kvp.Value.OwnerID));
                        parameters.Add("GroupID", MainClass.db.compressUUID(kvp.Value.GroupID));
                        parameters.Add("ParcelFlags", ((int)kvp.Value.Flags).ToString());
				
				        if(kvp.Value.AuthBuyerID==UUID.Zero)
				        {
					        parameters.Add("SalePrice",kvp.Value.SalePrice.ToString());
				        }
				        else
				        {
					        parameters.Add("SalePrice","-1");
				        }
				
                        MainClass.db.genericReplaceInto("Parcel", parameters, true);

                    });
                });


        }



        void Network_EventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            if (e.Simulator.Name != client.Network.CurrentSim.Name)
            {
				if(e.Simulator.Name=="")
					return;
				
                ThreadPool.QueueUserWorkItem(sync =>
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("Grid", MainClass.db.gridKey.ToString());
                    parameters.Add("Handle", e.Simulator.Handle.ToString());
                    parameters.Add("Name", e.Simulator.Name);
                    parameters.Add("Owner", MainClass.db.compressUUID(e.Simulator.SimOwner));

                    MainClass.db.genericInsertIgnore("Region", parameters);
                });
            }
            else
            {
               
 
            }
        }

        public void rotate()
        {
			return;
			
            angle = angle + (3.1415926 / 64.0);

            if (angle > 2.0 * 3.1415926)
                angle = 0;

            Vector3 newDirection;
            newDirection.X = (float)Math.Sin(angle); ;
            newDirection.Y = (float)Math.Cos(angle);
            newDirection.Z = (float)0;
            client.Self.Movement.TurnToward(newDirection);
            client.Self.Movement.SendUpdate(false);
 
        }

        public bool isConnected()
        {
            return client.Network.Connected;
        }

        public void Logout()
        {
            client.Network.Logout();
        }

        public int getObjectCount()
        {
            return client.Network.CurrentSim.ObjectsPrimitives.Count;
        }

        public int getSimObjectCount()
        {
            return client.Network.CurrentSim.Stats.Objects;
        }

        public int getRequestList()
        {
            return prop_request.Count;
        }

        public void localteleport(Vector3 pos)
        {
            Console.WriteLine("Trying to teleport intersim on "+client.Network.CurrentSim.Name+" to "+pos.ToString());
            bool status=client.Self.Teleport(client.Network.CurrentSim.Handle, pos);
            Console.WriteLine("Teleport status is " + status.ToString());
        }

        public Simulator getRegion()
        {
            return client.Network.CurrentSim;
        }

        public bool teleport(string region,Vector3 pos)
        {
            gotallparcels = false;
            return client.Self.Teleport(region, pos);
        }

        public bool teleport(Int64 handle, Vector3 pos)
        {
            gotallparcels = false;
            return client.Self.Teleport((ulong)handle, pos);
        }

    }
}
