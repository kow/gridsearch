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
		public int requests=0;
		
		public bool active;

        public NameTracker(GridClient conn)
        {
            client = conn;
            client.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_UUIDNameReply);
            agent_names_recieved = new List<UUID>();
		}

        void Avatars_UUIDNameReply(object sender, UUIDNameReplyEventArgs e)
        {
			if(active==false)
				return;
			
			foreach (KeyValuePair<UUID, string> kvp in e.Names)
			{
				 agent_names_recieved.Add(kvp.Key);
				 requests--;
			}
        }


        public void processagentID(UUID id)
        {
			
   		    if(active==false)
				return;

            if (id == UUID.Zero)
                return;
			
				if(agent_names_recieved.Contains(id)==false)
				{
					requests++;
					client.Avatars.RequestAvatarName(id);
				}
        }

        public bool complete()
        {

            if (requests <= 0)
                return true;

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
			requests=0;
				
		}



        
    }
}
