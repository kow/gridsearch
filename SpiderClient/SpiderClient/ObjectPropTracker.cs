using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using System.Threading;

namespace spider
{
    // ObjectPropTracker
    // This class tracks the object properties requests and tries to avoid flooding the simulator
    // and ensuring we get everything we have asked for

    class ObjectPropTracker
    {
         GridClient client;
        
         int timer_interval=5000;

         // // tracks localIDs per sim;
         Dictionary<ulong, Dictionary<uint,DateTime>> requests_props;
         Dictionary<ulong, List<uint>> active_requests_props;

         Dictionary<ulong,Dictionary<UUID, uint>> key_to_localID;

         Dictionary<ulong, Dictionary<UUID, DateTime>> requests_propsfamily;
         List<UUID> active_requests_propsfamily;

         List<UUID> intereset_list;

         static int MAX_NO_REQUESTS=50;
         static int MAX_WAIT_TIME = 60;

         int itterations=0;
		
		 DateTime start;
         Timer timer;

         public ObjectPropTracker(GridClient tclient)
         {
             client = tclient;

             requests_props = new Dictionary<ulong, Dictionary<uint, DateTime>>();
             active_requests_props = new Dictionary<ulong, List<uint>>();

             requests_propsfamily = new Dictionary<ulong, Dictionary <UUID, DateTime>>();
             active_requests_propsfamily = new List<UUID>();

             key_to_localID = new Dictionary<ulong,Dictionary<UUID, uint>>();

             intereset_list=new List<UUID>();

           
			 client.Objects.ObjectPropertiesFamily += new EventHandler<ObjectPropertiesFamilyEventArgs>(Objects_ObjectPropertiesFamily);
             client.Objects.ObjectProperties += new EventHandler<ObjectPropertiesEventArgs>(Objects_ObjectProperties);
             client.Objects.ObjectUpdate += new EventHandler<PrimEventArgs>(Objects_ObjectUpdate);
			 client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(TeleportUpdate);


             TimerCallback timerDelegate = new TimerCallback(ObjectTimerProc);
			 timer=new Timer(timerDelegate, null, timer_interval, timer_interval);
			
         }

		 void TeleportUpdate(object sender, TeleportEventArgs e)
		{
			Console.WriteLine("Teleport update :"+e.Message);
			start=DateTime.Now;
		}
		
         public void flush_for_new_sim()
         {
             requests_props.Clear();
             active_requests_props.Clear();
             requests_propsfamily.Clear();
             active_requests_propsfamily.Clear();
             key_to_localID.Clear();
             intereset_list.Clear();
             itterations = 0;
			 start=DateTime.Now;
         }

         void Objects_ObjectUpdate(object sender, PrimEventArgs e)
         {
             if(e.IsAttachment)
                 return;

             // we only care about new objects from now
             if (!e.IsNew)
                 return;
			
			 if(e.Simulator!=client.Network.CurrentSim)
				return;

             if (e.Prim.ParentID == 0)
             {
                 intereset_list.Add(e.Prim.ID);
                 itterations = 0;

                 ThreadPool.QueueUserWorkItem(sync =>
                 {

                     lock (key_to_localID)
                     {
                         if (!key_to_localID.ContainsKey(e.Simulator.Handle))
                         {
                             Dictionary<UUID, uint> map = new System.Collections.Generic.Dictionary<UUID, uint>();
                             map.Add(e.Prim.ID, e.Prim.LocalID);
                             key_to_localID.Add(e.Simulator.Handle, map);
                         }
                         else
                         {
                             try
                             {
                                 key_to_localID[e.Simulator.Handle].Add(e.Prim.ID, e.Prim.LocalID);
                             }
                             catch (Exception ee)
                             {
                                 Console.WriteLine(ee.Message);
                             }

                         }
                     }
                 });

                 ThreadPool.QueueUserWorkItem(sync =>
                 {
                     lock (requests_props)
                     {
                         if (!requests_props.ContainsKey(e.Simulator.Handle))
                         {
                             Dictionary<uint, DateTime> map = new System.Collections.Generic.Dictionary<uint, DateTime>();
                             requests_props.Add(e.Simulator.Handle, map);
                         }
                    
					    try
                        {
                             requests_props[e.Simulator.Handle].Add(e.Prim.LocalID, new DateTime(0));
					    }
					    catch(Exception eee)
					    {
						    Console.WriteLine(eee.Message);
					    }
                     }
                 });

                 ThreadPool.QueueUserWorkItem(sync =>
                 {
                     lock (requests_propsfamily)
                     {
                         if(!requests_propsfamily.ContainsKey(e.Simulator.Handle))
                         {
                             Dictionary<UUID, DateTime> map = new System.Collections.Generic.Dictionary<UUID,DateTime>();
                             requests_propsfamily.Add(e.Simulator.Handle,map);
                         }
                    
					     try
					     {
                         requests_propsfamily[e.Simulator.Handle].Add(e.Prim.ID, new DateTime(0));
					     }
					     catch(Exception eeee)
					    {
						    Console.WriteLine(eeee.Message);	
					    }
                     }
                 });
             }
         }

         void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)
         {

             ThreadPool.QueueUserWorkItem(sync =>
                 {

                     try
                     {
                         uint localID = key_to_localID[e.Simulator.Handle][e.Properties.ObjectID];

                         lock (active_requests_props)
                         {
                             Console.WriteLine("We got ObjectProperties for " + localID.ToString() + " remaining " + active_requests_props[e.Simulator.Handle].Count.ToString());
                             active_requests_props[e.Simulator.Handle].Remove(localID);
                             if (active_requests_props[e.Simulator.Handle].Count == 0)
                             {
                                 active_requests_props.Remove(e.Simulator.Handle);
                             }

                             MainClass.NameTrack.processagentID(e.Properties.OwnerID);
                             MainClass.NameTrack.processagentID(e.Properties.CreatorID);

                         }

                         lock (requests_props)
                         {
                             requests_props[e.Simulator.Handle].Remove(localID);
                             if (requests_props[e.Simulator.Handle].Count == 0)
                             {
                                 requests_props.Remove(e.Simulator.Handle);
                             }
                         }
                     }
                     catch (Exception ee)
                     {
                         Console.WriteLine("We got an ObjectProperties for an object we were not expecting.. ignoring\n" + ee.Message);
                     }
                 });
         }

         void Objects_ObjectPropertiesFamily(object sender, ObjectPropertiesFamilyEventArgs e)
         {
             ThreadPool.QueueUserWorkItem(sync =>
                 {
                     try
                     {

                         Console.WriteLine("We got ObjectPropertiesFamily for " + e.Properties.ObjectID.ToString() + " remaining " + active_requests_propsfamily.Count.ToString());
                         lock (active_requests_propsfamily)
                         {
                             active_requests_propsfamily.Remove(e.Properties.ObjectID);
                         }

                         lock (requests_propsfamily)
                         {
                             requests_propsfamily[e.Simulator.Handle].Remove(e.Properties.ObjectID);
                             if (requests_propsfamily[e.Simulator.Handle].Count == 0)
                             {
                                 requests_propsfamily.Remove(e.Simulator.Handle);
                             }
                         }

                     }
                     catch (Exception ee)
                     {
                         Console.WriteLine("We got an ObjectPropertiesFamily for an object we were not expecting.. ignoring" + ee.Message);
                     }
                 });
         }

         public void ObjectTimerProc(object state)
         {

            //Console.WriteLine("****** TICK OBJECTS *********");

			if(active_requests_props.Count>0)
			{
             	Console.WriteLine("Object props " + requests_props.Count.ToString() + "(" + active_requests_props.Count.ToString() + ")");
			}

             lock (requests_props) lock(active_requests_props)
             {
                 Dictionary<ulong, List<uint>> toupdate = new Dictionary<ulong, List<uint>>();

                 foreach (KeyValuePair<ulong, Dictionary<uint, DateTime>> kvp2 in requests_props)
                 {
                     List<uint> toupdate2 = new List<uint>();

                     Console.WriteLine("ObjectPropTracker, sim " + kvp2.Key.ToString() + " remaining = " + kvp2.Value.Count.ToString());
                     //if (kvp2.Value.Count < MAX_NO_REQUESTS)
                     {
                        
                         foreach (KeyValuePair<uint, DateTime> kvp in kvp2.Value)
                         {
                             DateTime req_time = kvp.Value;
                             req_time=req_time.AddSeconds(MAX_WAIT_TIME);
                             if (req_time.CompareTo(DateTime.Now) < 0)
                             {
                                 // This request is over 30s old we should process if we have space

                                 if (!active_requests_props.ContainsKey(kvp2.Key))
                                 {
                                     List<uint> map = new System.Collections.Generic.List<uint>();
                                     map.Add(kvp.Key);
                                     active_requests_props.Add(kvp2.Key, map);
                                 }

                                 if (!active_requests_props[kvp2.Key].Contains(kvp.Key))
                                 {
                                     if (active_requests_props[kvp2.Key].Count > MAX_NO_REQUESTS)
                                         break;

                                     active_requests_props[kvp2.Key].Add(kvp.Key);
                                 }

                                 toupdate2.Add(kvp.Key);

                                 Console.WriteLine("Requesting prop data for prim " + kvp.Key.ToString());
                                 Simulator thisobjectsim = client.Network.Simulators.Find(delegate(Simulator sim)
                                 {
                                     if (sim.Handle == kvp2.Key)
                                         return true;

                                     return false;
                                 });

                                 if (thisobjectsim != null)
                                 {
                                     client.Objects.SelectObject(thisobjectsim, kvp.Key, true);
                                  
                                 }
                                 else
                                 {
                                     Console.WriteLine("This object sim is NULL WTF, we were looking for "+kvp2.Key.ToString());
                                     //Possibly bad condition we have not got the sim we need anymore, this object will never
                                     //return
                                 }

                                 if (active_requests_props[kvp2.Key].Count >= 25)
                                     break;
                             }
                         }

                         toupdate.Add(kvp2.Key, toupdate2);
                     }
                 }

                 foreach (KeyValuePair<ulong, List<uint>> kvp in toupdate)
                 {
                     foreach (uint id in kvp.Value)
                     {
                         requests_props[kvp.Key][id] = DateTime.Now;
                     }

                 }
             }

			if(requests_propsfamily.Count>0 || active_requests_propsfamily.Count>0)
			{
             	Console.WriteLine("Object propsfamily " + requests_propsfamily.Count.ToString() + "(" + active_requests_propsfamily.Count.ToString() + ")");
			}

             lock (requests_propsfamily) lock (active_requests_propsfamily)
             {
                 Dictionary <ulong,List<UUID>> toupdate = new Dictionary<ulong,List<UUID>>();

                 foreach (KeyValuePair<ulong, Dictionary<UUID, DateTime>> kvp2 in requests_propsfamily)
                 {
                     List<UUID> toupdate2 = new List<UUID>();
                     foreach (KeyValuePair<UUID, DateTime> kvp in kvp2.Value)
                     {
                         DateTime req_time = kvp.Value;
                         req_time=req_time.AddSeconds(MAX_WAIT_TIME);

                         if (req_time.CompareTo(DateTime.Now) < 0)
                         {
                             if (!active_requests_propsfamily.Contains(kvp.Key))
                             {
                                 if (active_requests_propsfamily.Count > MAX_NO_REQUESTS)
                                     break;

                                 // This request is over 30s old we should process if we have space
                                 active_requests_propsfamily.Add(kvp.Key);
                             }

                             //requests_propsfamily[kvp.Key] = DateTime.Now;
                             toupdate2.Add(kvp.Key);

                             Console.WriteLine("Requesting family data for prim " + kvp.Key.ToString());

                             Simulator thisobjectsim = client.Network.Simulators.Find(delegate(Simulator sim)
                                 {
                                     if (sim.Handle == kvp2.Key)
                                         return true;

                                     return false;
                                 });

                             if (thisobjectsim != null)
                             {

                                 client.Objects.RequestObjectPropertiesFamily(thisobjectsim, kvp.Key, true);
                             }
                             else
                             {
                                 Console.WriteLine("This object sim is NULL WTF, we were looking for " + kvp2.Key.ToString());
                                 //Possibly bad condition we have not got the sim we need anymore, this object will never
                                 //return
                             }


                         }
                     }

                     toupdate.Add(kvp2.Key, toupdate2);
                 }

                 foreach (KeyValuePair<ulong, List<UUID>> kvp in toupdate)
                 {
                     foreach (UUID id in kvp.Value)
                     {
                         requests_propsfamily[kvp.Key][id] = DateTime.Now;
                     }
                 }
             }

             itterations++;

             //Timer t = (Timer)state;
             //t.Change(timer_interval, 0);
         }

         public void saveallprims()
         {
             client.Network.Simulators.ForEach(delegate(Simulator sim)
             {
                     sim.ObjectsPrimitives.ForEach(delegate(KeyValuePair<uint, Primitive> kvp)
                     {
                         if (kvp.Value.ParentID == 0)
                         {
                             if (kvp.Value.Properties != null)
                             {
                                 if (intereset_list.Contains(kvp.Value.ID) && kvp.Value.Properties.Name != "Object" && kvp.Value.Properties.Name != "")
                                 {
                                     //MainClass.db.updateObject(kvp.Value,client.Network.CurrentSim.Handle);
                                     Dictionary<string, string> parameters = new Dictionary<string, string>();
                                     parameters.Add("Grid", MainClass.db.gridKey.ToString());
                                     parameters.Add("LocalID", kvp.Value.LocalID.ToString());
                                     parameters.Add("Name", kvp.Value.Properties.Name);
                                     parameters.Add("Description", kvp.Value.Properties.Description);
                                     parameters.Add("ID", MainClass.db.compressUUID(kvp.Value.ID));
                                     parameters.Add("Region", client.Network.CurrentSim.Handle.ToString());
                                     parameters.Add("Creator", MainClass.db.compressUUID(kvp.Value.Properties.CreatorID));
                                     parameters.Add("Owner", MainClass.db.compressUUID(kvp.Value.Properties.OwnerID));
                                     parameters.Add("SalePrice", kvp.Value.Properties.SalePrice.ToString());
                                     parameters.Add("SaleType", ((int)kvp.Value.Properties.SaleType).ToString());
                                     parameters.Add("ParcelID", client.Parcels.GetParcelLocalID(client.Network.CurrentSim, kvp.Value.Position).ToString());

                                     int pos;
                                     pos = (int)kvp.Value.Position.X + ((int)kvp.Value.Position.Y * 255) + ((int)kvp.Value.Position.Z * 65535);
                                     parameters.Add("Location", pos.ToString());

                                     int children = 1; // account for root prim

                                     client.Network.CurrentSim.ObjectsPrimitives.ForEach(delegate(KeyValuePair<uint, Primitive> kvp2)
                                     {

                                         if (kvp2.Value.ParentID == kvp.Value.LocalID)
                                             children++;

                                     });

                                     parameters.Add("Prims", children.ToString());

                                     MainClass.db.genericReplaceInto("Object", parameters, true);
                                 }
                                 else
                                 {
                                     // Console.WriteLine("Not saving boring prim");
                                 }
                             }
                             else
                             {
                                 // Console.WriteLine("WTF NULL PROPERTIES!");
                             }
                         }
                     });
                 });

         }

         public bool complete()
         {

			TimeSpan span=DateTime.Now-start;
			
			if(span.Minutes>2)
					return true;
				
             if (active_requests_props.Count == 0 && active_requests_propsfamily.Count == 0 && span.Seconds > 15)
                 return true;

             return false;
         }


    }
}
