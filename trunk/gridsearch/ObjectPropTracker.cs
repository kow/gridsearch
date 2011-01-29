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

         public List<UUID> intereset_list;
        
         public List<UUID> requested_propsfamily;
         public List<UUID> requested_props;
         public bool active=false;
         int total_objects;
                 int total_linksets;
         int ignored_objects;		

         DateTime start;

         public ObjectPropTracker(GridClient tclient)
         {
             client = tclient;

             intereset_list=new List<UUID>();
            
             requested_props=new List<UUID>();
             requested_propsfamily=new List<UUID>();

             client.Objects.ObjectPropertiesFamily += new EventHandler<ObjectPropertiesFamilyEventArgs>(Objects_ObjectPropertiesFamily);
             client.Objects.ObjectProperties += new EventHandler<ObjectPropertiesEventArgs>(Objects_ObjectProperties);
             client.Objects.ObjectUpdate += new EventHandler<PrimEventArgs>(Objects_ObjectUpdate);
         }
        
         public void flush_for_new_sim()
         {
            requested_props.Clear();
            requested_propsfamily.Clear(); 
        intereset_list.Clear();
        start=DateTime.Now;
        total_objects = 0;
            total_linksets = 0;
         }

        
        
         void Objects_ObjectUpdate(object sender, PrimEventArgs e)
         {
        if(active==false)
            return;
            
             if(e.IsAttachment)
                 return;

             // we only care about new objects from now
             if (!e.IsNew)
                 return;
            
         if(e.Simulator!=client.Network.CurrentSim)
             return;

         total_objects++;

             if (e.Prim.ParentID == 0)
             {
                 total_linksets++;
                 intereset_list.Add(e.Prim.ID);
                 lock(requested_props)
             requested_props.Add(e.Prim.ID);
                
                 lock(requested_propsfamily)
             requested_propsfamily.Add(e.Prim.ID);
                
          client.Objects.RequestObjectPropertiesFamily(e.Simulator,e.Prim.ID);
          client.Objects.SelectObject(e.Simulator,e.Prim.LocalID);
         }
         }

        void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)
        {
            if(active==false)
                return;

            lock (requested_props)
            {
                if (requested_props.Contains(e.Properties.ObjectID))
                {
                    requested_props.Remove(e.Properties.ObjectID);
                    MainClass.NameTrack.processagentID(e.Properties.OwnerID);
                    MainClass.NameTrack.processagentID(e.Properties.CreatorID);
                }
            }
         }

         void Objects_ObjectPropertiesFamily(object sender, ObjectPropertiesFamilyEventArgs e)
         {
            
            if(active==false)
                return;

            lock (requested_propsfamily)
            {
                if (requested_propsfamily.Contains(e.Properties.ObjectID))
                {
                    requested_propsfamily.Remove(e.Properties.ObjectID);
                }
            }
         }
         
        public void saveallprims()
         {
        int count=0;
             //client.Network.Simulators.ForEach(delegate(Simulator sim)
             //{
                     MainClass.conn.client.Network.CurrentSim.ObjectsPrimitives.ForEach(delegate(KeyValuePair<uint, Primitive> kvp)
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
                                     parameters.Add("Perms", kvp.Value.Properties.Permissions.NextOwnerMask.ToString());

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
                     count++;
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

        Logger.Log("We have recorded "+count.ToString()+" linksets out of "+total_linksets.ToString()+" seen",Helpers.LogLevel.Info);
        Logger.Log("We processed "+total_objects.ToString()+" out of a possible "+client.Network.CurrentSim.Stats.Objects.ToString(),Helpers.LogLevel.Info);
         }

         public bool complete()
         {
            TimeSpan span=DateTime.Now-start;
            
            if (requested_props.Count==0 || requested_propsfamily.Count==0 && span.Seconds > 10)
                return true;

            return false;
         }


    }
}
