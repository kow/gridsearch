
using System;
using System.Threading;
using System.Collections.Generic;
using OpenMetaverse;

namespace spider
{
	class MainClass
	{
	   
	    //public static GridClient client;

        public static Database db;
        public static GridConn conn;
        public static ObjectPropTracker ObjTrack;
        public static NameTracker NameTrack;

	    static void Main()
	    {
            /*
            Console.WriteLine("As ulong "+x.GetULong().ToString());
            byte[] by= new byte[16];
            x.ToBytes(by,0);

            int a=0;
            for (a = 0; a < 16; a++)
            {
                String meh;
                meh=String.Format("{0:x} ",(int)by[a]);
                Console.Write(meh);
            }

            Console.WriteLine("As ulong " + by[0].ToString());

        System.Threading.Thread.Sleep(50000);
        return;
            */
             
        

        /*
               db.getlogin("Aditi");
               Int64 handle;

        
               string region= db.getNextRegionForGrid(out handle);
            */
         /*
               db.purgeobjects();
               db.purgeagents();
               db.purgeparcels();
               db.purgeregions();
            
               return;
            
             */

             
                   /*
                   UUID x = new UUID("21726045-e8f7-4b09-abd8-4bcc926e9e28");
               Console.WriteLine("UUID is " + x.ToString());

               string comp = db.compressUUID(x);
               Console.WriteLine("UUID Compressed is " + comp);
               Console.WriteLine("UUID UnCompressed is " + db.decompressUUID(comp).ToString());


               System.Threading.Thread.Sleep(50000);
               return;
                   /*
        
        
              */
			
		//while(true)
		{
				
		db = new Database();
        db.OpenDatabase();

        conn = new GridConn(db.getlogin("Agni"));

        if (conn.isConnected())
        {
            Console.WriteLine("We are logged in ok, proceed to scrape");
        }
        else
        {
            System.Threading.Thread.Sleep(1000*60*5);
            Console.WriteLine("Login failed, we should log this and move on");
            db.CloseDatabase();		
          
        }

        ObjTrack = new ObjectPropTracker(conn.client);
        NameTrack = new NameTracker(conn.client);
        Scraper scrape = new Scraper(conn.client);

			

		
		//System.Threading.Thread.Sleep(1000*60*5);
		}
      
        conn.Logout();
        db.CloseDatabase();
    
		Console.WriteLine("We all go byebye");
			
		System.Threading.Thread.Sleep(50000);
        
	    }

      
	    
	    
	}
}
