using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MySql.Data;
using MySql.Data.MySqlClient;
using OpenMetaverse;

namespace spider
{
    class Database
    {
        // This must be unique for each spider
        public int myid;

        public int gridKey;
        public bool regionsremaining = true;
        public bool gridhasregions = false;

        object thelock = new Object();

        string connStr;

        MySqlConnection conn;

        public bool OpenDatabase(string dbhost, string dbuser, string dbdatabase, string dbport, string dbpass)
        {

            if (dbhost == null)
            {
                Logger.Log("Host cannout be null for database", Helpers.LogLevel.Error);
                return false;
            }

            if (dbuser == null)
            {
                Logger.Log("User cannout be null for database", Helpers.LogLevel.Error);
                return false;
            }

            if (dbuser == null)
            {
                Logger.Log("User cannout be null for database", Helpers.LogLevel.Error);
                return false;
            }

            if (dbpass == null)
            {
                dbpass = ""; //empty password	
            }

            if (dbport == null)
            {
                dbport = "3306"; //mysql default	
            }

            connStr = string.Format("server={0};user={1};database={2};port={3};password={4};Allow Zero Datetime=True;", dbhost, dbuser, dbdatabase, dbport, dbpass);

            Random random = new Random();
            myid = random.Next();

            conn = new MySqlConnection(connStr);

            try
            {
                Logger.Log("Connecting to MySQL..", Helpers.LogLevel.Info);
                conn.Open();
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), Helpers.LogLevel.Error);
                return false;
            }

            Logger.Log("Connection open", Helpers.LogLevel.Info);
            return true;
        }

        public void CloseDatabase()
        {
            conn.Close();
        }

        public LoginParams getlogin(int gridkey)
        {
            LoginParams data = new LoginParams();

            // Remove any stale login locks, any over 30 minutes are quite dead

            int ProcessID = Process.GetCurrentProcess().Id;

            string sql;
            sql = "LOCK TABLES Logins WRITE, Grid READ; ";
            sql += "UPDATE Logins SET LockID='0', PID='0' WHERE (UNIX_TIMESTAMP(LastScrape)+1800) < UNIX_TIMESTAMP(NOW()); ";
            sql += "UPDATE Logins SET PID='" + ProcessID.ToString() + "', LastScrape=NOW(), LockID='" + myid.ToString() + "' WHERE LockID='0' AND grid='" + gridkey.ToString() + "' LIMIT 1;\n ";
            sql += "SELECT LoginURI, First, Last, Password, grid from Grid,Logins where Grid.PKey=Logins.grid and LockID ='" + myid.ToString() + "';";
            sql += "UNLOCK TABLES; ";

            MySqlDataReader rdr = null;

            try
            {

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error);
            }

            if (rdr.Read())
            {
                //Console.WriteLine(rdr[0] + " " + rdr[1] + " " + rdr[2] + " " + rdr[3]);
                data.URI = (string)rdr[0];
                data.FirstName = (string)rdr[1];
                data.LastName = (string)rdr[2];
                data.Password = (string)rdr[3];
                gridKey = (int)rdr[4];
            }
            else
            {
                // There are no free login slots to use on this grid
                Logger.Log("No free login slots left on grid id " + gridkey.ToString(), Helpers.LogLevel.Warning);
                data = null;
            }

            rdr.Close();

            return data;
        }

        public void clearlocks()
        {
            string sql = "";
            sql += "UPDATE Region SET LockID='0' WHERE LockID='" + myid.ToString() + "';\n";
            sql += "UPDATE Logins SET LockID='0' WHERE LockID='" + myid.ToString() + "';\n";

            lock (thelock)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Logger.Log("Error executing SQL when clearing locks " + e.Message, Helpers.LogLevel.Error);
                }
            }
        }

        public List<int> getFirstRegionGrid()
        {
            int grid = -1;
            List<int> possiblegrids = new List<int>();

            // This function needs to select a region that is older than the required threshold, the grid does not matter at this point because
            // what ever grid this returns will be used for the spider operation

            string sql = "";
            sql = "DROP TEMPORARY TABLE IF EXISTS candidates;";
            sql += "LOCK TABLES Region WRITE, Grid READ;";
            sql += "UPDATE Region SET LockID='0' WHERE LockID='" + myid.ToString() + "';\n"; // clean my lockids
            sql += "UPDATE Region SET LockID='0' WHERE LockID!='0' AND UNIX_TIMESTAMP(LastScrape)+3600 < UNIX_TIMESTAMP(NOW());"; //clean stale lockids
            sql += "CREATE TEMPORARY TABLE candidates SELECT * FROM Region WHERE LockID='0' AND UNIX_TIMESTAMP(LastScrape)+172800 < UNIX_TIMESTAMP(NOW());"; //2 weeks?
            sql += "INSERT INTO candidates (Grid) (SELECT PKey from Grid WHERE Grid.new='1');";
            sql += "SELECT Grid FROM candidates;";

            try
            {
                lock (thelock)
                {
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        object data = rdr[0];
                        if (data.GetType() != typeof(System.DBNull))
                        {
                            grid = (int)data;
                            if (!possiblegrids.Contains(grid))
                            {
                                possiblegrids.Add(grid);
                            }
                        }
                    }

                    rdr.Close();

                    sql = "DROP TEMPORARY TABLE candidates;";
                    sql += "UNLOCK TABLES; ";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error);
                sql = "DROP TEMPORARY TABLE IF EXISTS candidates;";
                sql += "UNLOCK TABLES; ";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            return possiblegrids;
        }


        public int getgridkey(string gridname)
        {
            int grid = 0;
            string sqlt = "SELECT PKey FROM Grid WHERE name='" + gridname + "';";
            MySqlDataReader rdr = null;
            try
            {
                lock (thelock)
                {
                    MySqlCommand cmd = new MySqlCommand(sqlt, conn);
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        grid = (int)rdr[0];
                    }
                }
            }
            catch
            {
                Logger.Log("Unable to find grid" + gridname, Helpers.LogLevel.Error);
            }

            rdr.Close();

            return grid;
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
                    lock (thelock)
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
                }
                catch
                {
                    gridhasregions = false;
                }

                if (gridhasregions == false)
                    return "";

            }

            // Lock the Region table then grab a lock on a region we would like to work with
            string sql = "";
            sql = "LOCK TABLES Region WRITE;\n";
            sql += "UPDATE Region SET LockID='0' WHERE LockID='" + myid.ToString() + "';\n";
            sql += "UPDATE Region SET LockID='0' WHERE LockID!='0' AND UNIX_TIMESTAMP(LastScrape)+3600 < UNIX_TIMESTAMP(NOW()) ;\n";
            sql += "UPDATE Region SET LastScrape=NOW(), LockID='" + myid.ToString() + "' WHERE LockID='0' AND Grid='" + gridKey.ToString() + "' AND UNIX_TIMESTAMP(LastScrape)+604800 < UNIX_TIMESTAMP(NOW()) ORDER BY LastScrape ASC LIMIT 1;\n";
            sql += "SELECT Name, Handle FROM Region WHERE LockID='" + myid.ToString() + "';\n";
            sql += "UNLOCK TABLES; ";

            try
            {
                lock (thelock)
                {
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();


                    if (rdr.Read())
                    {
                        object name = rdr[0];
                        if (name.GetType() == typeof(System.DBNull))
                        {
                            object rhandle = rdr[1];
                            Logger.Log("Grid has unnamed regions -> found " + rhandle.ToString(), Helpers.LogLevel.Info);
                            region = "";
                        }
                        else
                        {
                            Logger.Log("Grid has regions -> found " + (string)rdr[0], Helpers.LogLevel.Info);
                            region = (string)rdr[0];
                        }

                        object x = rdr[1];

                        handle = (Int64)x;
                        regionsremaining = true;

                    }
                    else
                    {
                        Logger.Log("Grid has no regions yet", Helpers.LogLevel.Info);
                        regionsremaining = false;
                    }

                    rdr.Close();

                }


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
            sql = sql + " WHERE ";
            sql = sql + addparams(conditions, "AND");
            sql = sql + ";";

            return ExecuteSQL(sql, parameters, conditions);
        }

        public bool genericUpdate2(string table, Dictionary<String, String> parameters, Dictionary<String, String> conditions)
        {
            string sql = "";

            sql = String.Format("UPDATE " + table + " SET ");

            sql = sql + addparams(parameters, ",");
            sql = sql + " WHERE ";
            sql = sql + addparams(conditions, "AND");
            sql = sql + ";";

            return ExecuteSQL(sql, parameters, conditions);
        }


        public bool updatescrape(string table, Dictionary<String, String> conditions)
        {

            string sql = String.Format("UPDATE " + table + " SET LastScrape=NOW() WHERE ");
            sql = sql + addparams(conditions, "AND");
            return ExecuteSQL(sql, null, conditions);
        }

        public bool genericReplaceInto(string table, Dictionary<String, String> parameters, bool updatescrape)
        {
            string sql = "";

            sql = String.Format("REPLACE INTO " + table + " SET ");
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


        public string addparams(Dictionary<String, String> parameters, string delimiter)
        {
            string sql = "";

            if (parameters.Count == 0)
                return "";

            foreach (KeyValuePair<String, String> kvp in parameters)
            {
                sql += kvp.Key + "=?" + kvp.Key + " " + delimiter + " ";
            }
            sql = sql.Substring(0, sql.Length - (delimiter.Length + 2));

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
            Logger.Log("SQL FAILED -> " + e.Message, Helpers.LogLevel.Error);
            Logger.Log("SQL was -> " + sql, Helpers.LogLevel.Error);
            if (parameters != null)
            {
                foreach (KeyValuePair<String, String> kvp in parameters)
                {
                    Logger.Log(kvp.Key + " ==> " + kvp.Value, Helpers.LogLevel.Error);
                }
            }

            System.Threading.Thread.Sleep(500000);
        }

        public bool ExecuteSQL(string sql, Dictionary<String, String> parameters, Dictionary<String, String> constraints)
        {
            lock (thelock)
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
            lock (thelock)
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
            input.ToBytes(compressed, 0);

            for (int x = 0; x < 16; x++)
            {
                compressedC[x] = (char)compressed[x];
            }

            return (new string(compressedC, 0, 16));
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
    }
}
