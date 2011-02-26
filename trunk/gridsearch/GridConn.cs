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
        public bool connected = false;
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
            login = client.Network.DefaultLoginParams(data.FirstName, data.LastName, data.Password, "GridSpider", "1.0");
            login.AgreeToTos = true;
            // login.Start = "home";
            login.URI = data.URI;
            login.Channel = "Test Grid Spider";
            login.Version = "1.0";

            client.Settings.OBJECT_TRACKING = true;
            client.Settings.PARCEL_TRACKING = true;
            client.Settings.ALWAYS_REQUEST_OBJECTS = false;
            client.Settings.SEND_AGENT_UPDATES = true;
            client.Settings.MULTIPLE_SIMS = true; // <-------------- very important indeed

            client.Network.SimDiscovered += new EventHandler<SimDiscoveredEventArgs>(Network_SimDiscovered);
            client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(Network_SimConnected);
            client.Network.SimConnecting += new EventHandler<SimConnectingEventArgs>(Network_SimConnecting);
            client.Parcels.SimParcelsDownloaded += new EventHandler<SimParcelsDownloadedEventArgs>(Parcels_SimParcelsDownloaded);
            client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.Disconnected += new EventHandler<DisconnectedEventArgs>(Network_Disconnected);
            client.Network.LoggedOut += new EventHandler<LoggedOutEventArgs>(Network_LoggedOut);
            client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
            //client.Grid.GridLayer += new EventHandler<GridLayerEventArgs>(Grid_GridLayer);
            //client.Grid.GridRegion += new EventHandler<GridRegionEventArgs>(Grid_GridRegion);


            client.Self.ChatFromSimulator += HandleClientSelfChatFromSimulator;
            client.Self.IM += HandleClientSelfIM;

            client.Self.Movement.Camera.Far = 512;

            client.Network.Login(login);

            client.Self.Movement.Camera.Far = 512;

            client.Self.Movement.SendUpdate(true);

            if (client.Network.LoginStatusCode == LoginStatus.Success)
            {
                Logger.Log("Login procedure completed", Helpers.LogLevel.Info);
                connected = true;
            }
            Logger.Log("Status is " + client.Network.LoginStatusCode.ToString(), Helpers.LogLevel.Info);
            Logger.Log(client.Network.LoginMessage, Helpers.LogLevel.Info);

        }

        void Grid_GridRegion(object sender, GridRegionEventArgs e)
        {
            Logger.Log(" *** New grid region data ", Helpers.LogLevel.Info);

        }

        void Grid_GridLayer(object sender, GridLayerEventArgs e)
        {

            Logger.Log(" *** New grid layer data " + e.Layer.Left.ToString() + "," + e.Layer.Right.ToString(), Helpers.LogLevel.Info);
        }

        void Network_SimDiscovered(object sender, SimDiscoveredEventArgs e)
        {
            Logger.Log("*** Sim Discovered " + e.Simulator.Handle + " " + e.Simulator.Name, Helpers.LogLevel.Info);

            ThreadPool.QueueUserWorkItem(sync =>
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Dictionary<string, string> conditions = new Dictionary<string, string>();
                parameters.Add("Grid", MainClass.db.gridKey.ToString());
                parameters.Add("Handle", e.Simulator.Handle.ToString());
                parameters.Add("Name", e.Simulator.Name);
                parameters.Add("Owner", MainClass.db.compressUUID(e.Simulator.SimOwner));

                MainClass.db.genericInsertIgnore("Region", parameters);
            });

        }

        void Network_SimConnecting(object sender, SimConnectingEventArgs e)
        {
            Logger.Log("Sim connecting " + e.Simulator.Handle + " " + e.Simulator.Name, Helpers.LogLevel.Info);
        }

        void HandleClientSelfIM(object sender, InstantMessageEventArgs e)
        {

        }

        void HandleClientSelfChatFromSimulator(object sender, ChatEventArgs e)
        {

        }

        void Network_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Log("*** BONED WE HAVE BEEN BOOTED ***", Helpers.LogLevel.Error);
            Logger.Log(e.Message, Helpers.LogLevel.Error);
            Logger.Log(e.Reason, Helpers.LogLevel.Error);

            if (connected == true)
            {
                connected = false;
                client.Network.Logout(); //force logout to clean up libomv
            }
        }

        void Network_SimDisconnected(object sender, SimDisconnectedEventArgs e)
        {
            Logger.Log("Sim disconnected from " + e.Simulator.Name + " Reason " + e.Reason, Helpers.LogLevel.Info);
            if (client.Network.CurrentSim == e.Simulator)
            {
                Logger.Log("*** BONED WE HAVE BEEN BOOTED ***", Helpers.LogLevel.Error);

                if (connected == true)
                {
                    connected = false;
                    client.Network.Logout(); //force logout to clean up libomv
                }
            }
        }

        void Network_LoggedOut(object sender, LoggedOutEventArgs e)
        {
            Logger.Log("***** LOGOUT RECIEVED ITS ALL OVER ********", Helpers.LogLevel.Error);
            connected = false;
        }

        void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            Logger.Log("New sim connection from " + e.Simulator.Name, Helpers.LogLevel.Info);


            if (client.Network.CurrentSim.Handle == e.Simulator.Handle)
            {
                Logger.Log("Current sim connection, we are now in " + e.Simulator.Name, Helpers.LogLevel.Info);

            }
        }

        void Self_TeleportProgress(object sender, TeleportEventArgs e)
        {
            Logger.Log("TP Update --> " + e.Message.ToString() + " : " + e.Status.ToString() + " : " + e.Flags.ToString(), Helpers.LogLevel.Info);

            if (e.Status == TeleportStatus.Finished)
            {
                Logger.Log("TP Finished we are now in " + client.Network.CurrentSim.Name, Helpers.LogLevel.Info);
            }
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
                    parameters.Add("MusicURL", kvp.Value.MusicURL);
                    parameters.Add("MediaURL", kvp.Value.Media.MediaURL);
		

                    if (kvp.Value.AuthBuyerID == UUID.Zero)
                    {
                        parameters.Add("SalePrice", kvp.Value.SalePrice.ToString());
                    }
                    else
                    {
                        parameters.Add("SalePrice", "-1");
                    }

                    MainClass.db.genericReplaceInto("Parcel", parameters, true);
                });
            });
        }

        public void rotate()
        {
            angle = angle + (3.1415926 / 16);

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
            try
            {
                client.Network.Logout();
            }
            catch (Exception e)
            {
                Logger.Log("Logout exploded.. again .." + e.Message, Helpers.LogLevel.Error);
            }
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
            bool status = client.Self.Teleport(client.Network.CurrentSim.Handle, pos);
            Logger.Log("Teleport status is " + status.ToString(), Helpers.LogLevel.Info);
        }

        public Simulator getRegion()
        {
            return client.Network.CurrentSim;
        }

        public bool teleport(string region, Vector3 pos)
        {
            gotallparcels = false;
            return client.Self.Teleport(region, pos);
        }

        public bool teleport(Int64 handle, Vector3 pos)
        {
            gotallparcels = false;
            return client.Self.Teleport((ulong)handle, pos);
        }

        public void mapwalk()
        {
            /*
            speculateregions();
            ThreadPool.QueueUserWorkItem(sync =>
            {

                Logger.Log("*** Starting map walk for sim on 6x6 grid", Helpers.LogLevel.Info);
   
                List<MapItem> map = null;

                int x, y;

                Vector3d gpos = MainClass.conn.client.Self.GlobalPosition;

                for (x = -5; x < 5; x++)
                    for (y = -5; y < 5; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

            map = null;
                        map = client.Grid.MapItems(Utils.UIntsToLong((uint)(gpos.X + x * 256), (uint)(gpos.Y + y * 256)), GridItemType.AgentLocations, GridLayerType.Objects, 250);

                        if (map != null)
                        {
                            //Logger.Log("*** Block request for " + x.ToString() + " " + y.ToString() + " gave back " + map.Count(), Helpers.LogLevel.Info);

                            float localX, localY;
                            ulong region = Helpers.GlobalPosToRegionHandle((float)gpos.X + x * 256, (float)gpos.Y + y * 256, out localX, out localY);

                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                            Dictionary<string, string> conditions = new Dictionary<string, string>();
                            parameters.Add("Grid", MainClass.db.gridKey.ToString());
                            parameters.Add("Handle", region.ToString());
                            MainClass.db.genericInsertIgnore("Region", parameters);
                        }
                        else
                        {
                            Logger.Log("*** Block request for " + x.ToString() + " " + y.ToString() + " gave back a null map ", Helpers.LogLevel.Info);
                        }
                    }
                Logger.Log("*** Map walk complete ", Helpers.LogLevel.Info);
            });
             */
        }

        public void speculateregions()
        {
            ThreadPool.QueueUserWorkItem(sync =>
                {

                    //*sigh* opensim fails to report neighbours correctly via standard sim connects unless you are very close to a region
                    // and the mapitems does not work either so just add the 8 neighbout blocks to the spider list
                    // worst case is that we get some extra cruff in the region database

                    Vector3d gpos = MainClass.conn.client.Self.GlobalPosition;
                    int x;
                    int y;

                    for (x = -1; x <= 1; x++)
                        for (y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0)
                                continue;
                            Dictionary<string, string> parameters = new Dictionary<string, string>();

                            float localX, localY;
                            ulong region = Helpers.GlobalPosToRegionHandle((float)gpos.X + x * 256, (float)gpos.Y + y * 256, out localX, out localY);
                            parameters.Add("Grid", MainClass.db.gridKey.ToString());
                            parameters.Add("Handle", region.ToString());
                            MainClass.db.genericInsertIgnore("Region", parameters);
                        }

                });

        }

    }
}
