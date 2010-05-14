using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using System.Threading;

namespace spider
{
    class NameTracker
    {
        GridClient client;

        List<UUID> agent_names_recieved;
        public Dictionary<UUID, DateTime> agent_names_requested;

		public bool active;

        public NameTracker(GridClient conn)
        {
            client = conn;
            client.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_UUIDNameReply);
            agent_names_recieved = new List<UUID>();
            agent_names_requested = new Dictionary<UUID, DateTime>();
		}

        void Avatars_UUIDNameReply(object sender, UUIDNameReplyEventArgs e)
        {
			if(active==false)
				return;
			
			foreach (KeyValuePair<UUID, string> kvp in e.Names)
			{
				 agent_names_recieved.Add(kvp.Key);

                 if(agent_names_requested.ContainsKey(kvp.Key))
                 {
                     agent_names_requested.Remove(kvp.Key);
                 }
			}
        }


        public void processagentID(UUID id)
        {
			
   		    if(active==false)
				return;

            if (id == UUID.Zero)
                return;

            if (agent_names_recieved.Contains(id) == false && agent_names_requested.ContainsKey(id)==false)
				{
                    lock (agent_names_requested)
                    {
                        agent_names_requested.Add(id, DateTime.Now);
                    }
				}
        }

        public bool complete()
        {

            if (agent_names_requested.Count == 0)
                return true;

            List<UUID> rerequest = new List<UUID>();

            lock (agent_names_requested)
            {
                foreach (KeyValuePair<UUID, DateTime> kvp in agent_names_requested)
                {
                    TimeSpan span = DateTime.Now - kvp.Value;
                    if (span.Seconds > 20)
                        rerequest.Add(kvp.Key);
                }
            }

            if (rerequest.Count > 0)
            {
                client.Avatars.RequestAvatarNames(rerequest);
            }

            return false;
        }
		
		
		public void savenamestodb()
		{
			
			foreach(UUID id in agent_names_recieved)
			{
				 Console.WriteLine("Adding " + id.ToString() + " to name request queue");
                 Dictionary<string, string> parameters = new Dictionary<string, string>();
                 parameters.Add("AgentID", MainClass.db.compressUUID(id));
                 parameters.Add("Grid", MainClass.db.gridKey.ToString());
                 MainClass.db.genericInsertIgnore("Agent", parameters);
 	
			}
			
			agent_names_recieved.Clear();
            agent_names_requested.Clear();		
		}



        
    }
}
