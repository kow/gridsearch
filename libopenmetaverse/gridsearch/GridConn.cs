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
        List<ulong> discovered_sims;
        public bool gotallparcels = false;

        public GridConn(LoginParams data)
        {
            client = new GridClient();
            prop_request = new List<UUID>();

              discovered_sims = new List<ulong>();

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
            client.Settings.MULTIPLE_SIMS = false; // <-------------- very important to be false!

            client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(Network_SimConnected);
            client.Parcels.SimParcelsDownloaded += new EventHandler<SimParcelsDownloadedEventArgs>(Parcels_SimParcelsDownloaded);
            client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.Disconnected += new EventHandler<DisconnectedEventArgs>(Network_Disconnected);
            client.Network.LoggedOut +=	new EventHandler<LoggedOutEventArgs>(Network_LoggedOut);
            client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
            
            client.Self.ChatFromSimulator += HandleClientSelfChatFromSimulator;	
            client.Self.IM += HandleClientSelfIM;

            client.Self.Movement.Camera.Far = 1024;
            
            client.Network.Login(login);

            client.Self.Movement.Camera.Far = 1024;
            client.Self.Movement.SendUpdate(true);


            if (client.Network.LoginStatusCode == LoginStatus.Success)
            {
                Logger.Log("Login procedure completed", Helpers.LogLevel.Info);
                connected=true;
            }
            Logger.Log("Status is " + client.Network.LoginStatusCode.ToString(), Helpers.LogLevel.Info);
            Logger.Log(client.Network.LoginMessage, Helpers.LogLevel.Info);

        }
        
        void HandleClientSelfIM (object sender, InstantMessageEventArgs e)
        {
            
        }

        void HandleClientSelfChatFromSimulator (object sender, ChatEventArgs e)
        {
                  
        }
        
        void doLogout()
        {
                try
                {
                    Logger.Log("Trying to logout", Helpers.LogLevel.Info);
                    client.Network.Logout(); //force logout to clean up libomv
                }
                catch(Exception e)
                {
                     Logger.Log("Logout exploded in a heap :"+e.Message, Helpers.LogLevel.Error);					 
                }
        
        }

        void Network_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Log("*** BONED WE HAVE BEEN BOOTED ***", Helpers.LogLevel.Error);
            Logger.Log(e.Message, Helpers.LogLevel.Error);
            Logger.Log(e.Reason, Helpers.LogLevel.Error);

            if (connected == true)
            {
                connected = false;
                doLogout();
            }
        }
        
        void Network_SimDisconnected(object sender, SimDisconnectedEventArgs e)
        {
            Logger.Log("Sim disconnected from " + e.Simulator.Name + " Reason " + e.Reason, Helpers.LogLevel.Info);
            if(client.Network.CurrentSim==e.Simulator)
            {
                Logger.Log("*** BONED WE HAVE BEEN BOOTED ***", Helpers.LogLevel.Error);

                if (connected == true)
                {
                    connected = false;
                    doLogout(); //force logout to clean up libomv
                }
            }	
        }
        
        void Network_LoggedOut(object sender, LoggedOutEventArgs e)
        {
            Logger.Log("***** LOGOUT RECIEVED ITS ALL OVER ********", Helpers.LogLevel.Error);
            connected=false;
        }

        void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
             Logger.Log("New sim connection from " + e.Simulator.Name, Helpers.LogLevel.Info);
                    
            ThreadPool.QueueUserWorkItem(sync =>
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Dictionary<string, string> conditions = new Dictionary<string, string>();
                conditions.Add("Grid", MainClass.db.gridKey.ToString());
                conditions.Add("Handle", e.Simulator.Handle.ToString());
                parameters.Add("Name", e.Simulator.Name);
                parameters.Add("Owner", MainClass.db.compressUUID(e.Simulator.SimOwner));
                    
                MainClass.db.genericUpdate("Region", parameters,conditions);
            });
        }

        void Self_TeleportProgress(object sender, TeleportEventArgs e)
        {
               Console.WriteLine();
               Logger.Log("TP Update --> "+e.Message.ToString()+" : "+e.Status.ToString(),Helpers.LogLevel.Info);
        }

        void Parcels_SimParcelsDownloaded(object sender, SimParcelsDownloadedEventArgs e)
        {
            if (gotallparcels == true)
                return;

             gotallparcels = true;
             Logger.Log("Got all parcels, writing to db", Helpers.LogLevel.Info);

                ThreadPool.QueueUserWorkItem(sync =>
                {
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

        public void rotate()
        {		
            angle = angle + (3.1415926 / 4);

            if (angle > 2.0 * 3.1415926)
                angle = 0;

            Vector3 newDirection;
            newDirection.X = (float)Math.Sin(angle); ;
            newDirection.Y = (float)Math.Cos(angle);
            newDirection.Z = (float)0;
            client.Self.Movement.TurnToward(newDirection);
 
        }

        public bool isConnected()
        {
            return client.Network.Connected;
        }

        public void Logout()
        {
            doLogout();
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
            Logger.Log("Trying to teleport intersim on " + client.Network.CurrentSim.Name + " to " + pos.ToString(), Helpers.LogLevel.Info);
            Console.WriteLine();
            bool status=client.Self.Teleport(client.Network.CurrentSim.Handle, pos);
            Logger.Log("Teleport status is " + status.ToString(),Helpers.LogLevel.Info);
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
