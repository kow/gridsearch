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

        List<UUID> agent_names_wait;
        Dictionary<UUID, DateTime> agent_names_queue;
        List<UUID> agent_names_recieved;

        int itterations = 0;
		static int timer_interval=2000;
        Timer timer;

        public NameTracker(GridClient conn)
        {
            client = conn;
            client.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_UUIDNameReply);
            agent_names_recieved = new List<UUID>();
            agent_names_wait = new List<UUID>();
            agent_names_queue = new Dictionary<UUID, DateTime>();

            TimerCallback timerDelegate = new TimerCallback(NameTimerProc);
			timer=new Timer(timerDelegate, null, timer_interval, timer_interval);
        }

        void Avatars_UUIDNameReply(object sender, UUIDNameReplyEventArgs e)
        {
            itterations = 0;

            ThreadPool.QueueUserWorkItem(sync =>
            {
                     Console.WriteLine("Recieved " + e.Names.Count.ToString() + " AV names");

                     foreach (KeyValuePair<UUID, string> kvp in e.Names)
                     {
                         Console.WriteLine("Got " + kvp.Value.ToString());

                         lock (agent_names_wait) lock (agent_names_recieved) lock (agent_names_queue)
                                 {
                                     if (!agent_names_recieved.Contains(kvp.Key) && !agent_names_wait.Contains(kvp.Key))
                                     {
                                         if (agent_names_queue.ContainsKey(kvp.Key))
                                         {
                                             Console.WriteLine("Got names for " + kvp.Value);

                                             Dictionary<string, string> parameters = new Dictionary<string, string>();
                                             Dictionary<string, string> conditions = new Dictionary<string, string>();

                                             conditions.Add("AgentID", MainClass.db.compressUUID(kvp.Key));
                                             conditions.Add("Grid", MainClass.db.gridKey.ToString());
                                             parameters.Add("Name", kvp.Value);
                                             MainClass.db.genericUpdate("Agent", parameters, conditions);

                                             agent_names_recieved.Add(kvp.Key);
                                             agent_names_queue.Remove(kvp.Key);

                                         }
                                     }
                                 }
                     }
                 });
        }


        public void processagentID(UUID id)
        {


            if (id == UUID.Zero)
                return;

            ThreadPool.QueueUserWorkItem(sync =>
                 {

                     itterations = 0;

                     lock (agent_names_wait) lock (agent_names_recieved) lock (agent_names_queue)
                     if (!agent_names_recieved.Contains(id) && !agent_names_queue.ContainsKey(id) && !agent_names_wait.Contains(id))
                     {
                         Console.WriteLine("Adding " + id.ToString() + " to name request queue");
                         Dictionary<string, string> parameters = new Dictionary<string, string>();
                         parameters.Add("AgentID", MainClass.db.compressUUID(id));
                         parameters.Add("Grid", MainClass.db.gridKey.ToString());
                         MainClass.db.genericInsertIgnore("Agent", parameters);
 
                         agent_names_wait.Add(id);
 
                     }
                     else
                     {
                         Console.WriteLine("not adding name request to queue, already in progress");

                     }
                 });
        }


        public static void NameTimerProc(object state)
        {
            //Console.WriteLine("**** TICK NAME *****");

            if (MainClass.NameTrack.agent_names_wait.Count > 0 || MainClass.NameTrack.agent_names_queue.Count > 0)
			{
                Console.WriteLine("Names queue wait " + MainClass.NameTrack.agent_names_wait.Count.ToString() + "/" + MainClass.NameTrack.agent_names_queue.Count.ToString() + "/" + MainClass.NameTrack.agent_names_recieved.Count.ToString());
			}
			
            List<UUID> names = new List<UUID>();

            lock (MainClass.NameTrack.agent_names_wait)
            {
                foreach (UUID id in MainClass.NameTrack.agent_names_wait)
                {
                    names.Add(id);
                    MainClass.NameTrack.agent_names_queue[id] = DateTime.Now;
                }

                foreach (UUID id in names)
                {
                    MainClass.NameTrack.agent_names_wait.Remove(id);
                }
            }

            lock (MainClass.NameTrack.agent_names_queue)
            {
                foreach (KeyValuePair<UUID, DateTime> kvp in MainClass.NameTrack.agent_names_queue)
                {
                    if (DateTime.Now.CompareTo(kvp.Value.AddSeconds(60)) > 0)
                    {
                        names.Add(kvp.Key); 
                    }

                }

                foreach (UUID id in names)
                {
                    MainClass.NameTrack.agent_names_queue[id] = DateTime.Now;
                }

            }

			if(names.Count>0)
			{
				Console.WriteLine("Requesting " + names.Count.ToString() + " AV names");
                foreach (UUID id in names)
                {
                    Console.WriteLine("Requesting " + id.ToString());
                }
            	MainClass.conn.client.Avatars.RequestAvatarNames(names);
			}

            MainClass.NameTrack.itterations++;
            // The state object is the Timer object.
            //Timer t = (Timer)state;
            //t.Change(10000, 0);

        }

        public bool complete()
        {

            if (agent_names_queue.Count == 0 && agent_names_wait.Count==0 && itterations > 10)
                return true;

            return false;
        }



        
    }
}
