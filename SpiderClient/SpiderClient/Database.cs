using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using OpenMetaverse;

namespace spider
{
    class Database
    {

        public int gridKey;
        public bool regionsremaining = true;
        public bool gridhasregions = false;
		
		List<int> thelock;


        string connStr = "server=cornelius.demon.co.uk;user=spiderer;database=spider;port=3306;password=louise42;Allow Zero Datetime=True;";
        MySqlConnection conn;

        public bool OpenDatabase()
        {
            
			thelock=new List<int>();
            conn = new MySqlConnection(connStr);
        
            try
		    {
		        Console.WriteLine("Connecting to MySQL..");
		        conn.Open();
		    }
		    catch (Exception e)
		    {
		        Console.WriteLine(e.ToString());
                return false;
		    }

            Console.WriteLine("Connection open");
            return true;
        }

        public void CloseDatabase()
        {
            conn.Close();	
        }

        public LoginParams getlogin(string gridname)
        {
           
            string sql = "SELECT LoginURI, First, Last, Password, PKey from Grid where name ='" + gridname + "';";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            LoginParams data=new LoginParams();

            // FIXME this is shit
            if (rdr.Read())
            {
                Console.WriteLine(rdr[0] + " " + rdr[1] + " " + rdr[2] + " " + rdr[3]);
                data.URI = (string)rdr[0];
                data.FirstName = (string)rdr[1];
                data.LastName = (string)rdr[2];
                data.Password = (string)rdr[3];
                gridKey = (int)rdr[4];
            }

            rdr.Close();

            return data;
        }

  
        public string getNextRegionForGrid(out Int64 handle)
        {
            
            string region = "";
            handle = 0;

            if (gridhasregions == false)
            {

                string sqlt = "SELECT Name FROM Region WHERE Grid='" + gridKey.ToString() + "';";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(sqlt, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        gridhasregions = true;
                    }
                    else
                    {
                        gridhasregions = false;
                    }

                    rdr.Close();
                }
                catch (Exception e)
                {
                    gridhasregions = false;
                }

                if (gridhasregions == false)
                    return "";

            }
            
            // This must be unique for each spider
            int myid = 754758;

            // Lock the Region table then grab a lock on a region we would like to work with
            string sql="";
            sql =  "LOCK TABLES Region WRITE;\n";
            sql += "UPDATE Region SET LockID='0' WHERE LockID='" + myid.ToString() + "';\n";
            sql += "UPDATE Region SET LockID='" + myid.ToString() + "' WHERE LockID='0' AND Grid='" + gridKey.ToString() + "' AND UNIX_TIMESTAMP(LastScrape)+604800 < UNIX_TIMESTAMP(NOW()) ORDER BY LastScrape ASC LIMIT 1;\n";            
            sql += "SELECT Name, Handle FROM Region WHERE LockID='" + myid.ToString()+"';\n";
            sql += "UNLOCK TABLES; ";

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    Console.WriteLine("Grid has regions -> found "+(string)rdr[0]);
                    region=(string)rdr[0];
                    object x = rdr[1];
                    Console.WriteLine(x.GetType().ToString());
                    handle = (Int64)x;
                    regionsremaining = true;
              
                }
                else
                {
                    Console.WriteLine("Grid has no regions yet");
                    regionsremaining = false;
                }

                rdr.Close();
               

            }
            catch (Exception e)
            {
                dumpErrMsg(e, sql, null);
            }

            return region;

        }

        public bool genericUpdate(string table, Dictionary<String, String> parameters, Dictionary<String, String> conditions)
        {
            string sql = "";

            sql = String.Format("UPDATE " + table + " SET LastScrape=NOW() ");
            if (parameters.Count > 0)
            {
                sql += ", ";
            }

            sql = sql + addparams(parameters, ",");
            sql=sql+" WHERE ";
            sql = sql + addparams(conditions, "AND");
            sql=sql+";";
            
            return ExecuteSQL(sql, parameters, conditions);
        }
        
        public bool updatescrape(string table, Dictionary<String,String>conditions)
        {

            string sql = String.Format("UPDATE " + table + " SET LastScrape=NOW() WHERE ");
            sql = sql + addparams(conditions, "AND");
            return ExecuteSQL(sql, null, conditions);
        }

        public bool genericReplaceInto(string table,Dictionary<String,String>parameters,bool updatescrape)
        {
            string sql="";

            sql = String.Format("REPLACE INTO "+table+" SET ");
            sql = sql + addparams(parameters, ",");
        
            if (updatescrape)
            {
                sql = sql + ", LastScrape=NOW();";
            }

            return ExecuteSQL(sql, parameters, null);
        }

        public bool genericInsertIgnore(string table, Dictionary<String, String> parameters)
        {
            string sql = "";
            sql = String.Format("INSERT IGNORE INTO " + table + " SET ");
            sql = sql + addparams(parameters, ",");
            return ExecuteSQL(sql, parameters, null);
        }

        public void purgeobjects()
        {
            string sql = String.Format("DELETE FROM Object;");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void purgeagents()
        {
            string sql = String.Format("DELETE FROM Agent;");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void purgeparcels()
        {
            string sql = String.Format("DELETE FROM Parcel;");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void purgeregions()
        {
            string sql = String.Format("DELETE FROM Region;");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }


        public string addparams(Dictionary<String, String> parameters,string delimiter)
        {
            string sql="";

            foreach (KeyValuePair<String, String> kvp in parameters)
            {
                sql += kvp.Key + "=?" + kvp.Key + " "+delimiter+" ";
            }
            sql = sql.Substring(0, sql.Length - (delimiter.Length+2));

            return sql;
        }

        public MySqlCommand buildsql(string sql, Dictionary<String, String> parameters, Dictionary<String, String> conditions)
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (KeyValuePair<String, String> kvp in parameters)
                {
                    cmd.Parameters.AddWithValue("?" + kvp.Key, kvp.Value);
                }
            }

            if (conditions != null)
            {
                foreach (KeyValuePair<String, String> kvp in conditions)
                {
                    cmd.Parameters.AddWithValue("?" + kvp.Key, kvp.Value);
                }
            }

            return cmd;
        }

        public void dumpErrMsg(Exception e, string sql, Dictionary<String, String> parameters)
        {
            Console.WriteLine("*********************************************");
            Console.WriteLine("\nSql failed " + e.Message);
            Console.WriteLine(sql + "\n");
            if (parameters != null)
            {
                foreach (KeyValuePair<String, String> kvp in parameters)
                {
                    Console.WriteLine(kvp.Key + "=" + kvp.Value);
                }
            }
            Console.WriteLine("\n");
            Console.WriteLine("*********************************************");

            

            System.Threading.Thread.Sleep(500000);
        }

        public bool ExecuteSQL(string sql, Dictionary<String, String> parameters, Dictionary<String, String> constraints)
        {	
			//lock(thelock)
			{
	            try
	            {
	                MySqlCommand cmd = buildsql(sql, parameters, constraints);
	                lock (conn)
	                {
	                    cmd.ExecuteNonQuery();
	                }
	                return true;
	
	            }
	            catch (Exception e)
	            {
	                dumpErrMsg(e, sql, parameters);
	                return false;
	            }
			}

        }

        public bool ExecuteQuersy(string sql, Dictionary<String, String> parameters, Dictionary<String, String> constraints, out MySqlDataReader rdr)
        {
			//lock(thelock)
			{
	            try
	            {
	                MySqlCommand cmd = new MySqlCommand(sql, conn);
	                rdr = cmd.ExecuteReader();
	                return true;
	            }
	            catch (Exception e)
	            {
	                rdr = null;
	                dumpErrMsg(e, sql, parameters);
	                return false;
	            }
			}
        }

        public string compressUUID(UUID input)
        {
            byte[] compressed = new byte[16];
            char[] compressedC = new char[16]; 
            input.ToBytes(compressed,0);

            for (int x = 0; x < 16; x++)
            {
                compressedC[x] = (char)compressed[x];
            }

            return (new string(compressedC,0,16));
        }

        public UUID decompressUUID(string input)
        {
            char[] compressed = input.ToCharArray();
            byte[] bc = new byte[16];
            for (int x = 0; x < 16; x++)
            {
                bc[x] = (byte)compressed[x];
            }
            return new UUID(bc, 0);
        }

        /*
        public TimeSpan agentexists(UUID key)
        {
            MySqlDataReader rdr;
            string sql = "SELECT LastScrape FROM Agent WHERE AgentID='" + compressUUID(key) + "';\n";
            ExecuteQuery(sql,null,null,out rdr);

            if (rdr.Read())
            {
                TimeSpan ret;
               
                ret = (TimeSpan)rdr[0];
                rdr.Close();
                return ret;
            }
            else
            {
                rdr.Close();
                return new DateTime(1970, 1, 1, 12, 0, 0, 0); ;
            }
        }
         * */

    
    }
}
