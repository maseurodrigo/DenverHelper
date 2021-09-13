using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace DiscordDenver.Data.MySQL
{
    public class MySQLComms 
    {
        // Check if discord bot custom command already exists on DB
        public static async Task<bool> checkCommExists(MySqlConnection _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@Comm", _Comm);
                int result = Convert.ToInt32(await mySQLComm.ExecuteScalarAsync());
                if (result > 0) return true;
                else return false;
            }
        }

        // Get command data from DB for the current server
        public static async Task<String> getCommData(MySqlConnection _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT Data FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@Comm", _Comm);
                using (MySqlDataReader reader = await mySQLComm.ExecuteReaderAsync()) {
                    await reader.ReadAsync();
                    return reader.GetString(0);
                }
            }
        }

        // Get the amount of commands on DB for the current server
        public static async Task<int> getTotalComms(MySqlConnection _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_Comms WHERE ServerID=@ServerID";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                return Convert.ToInt32(await mySQLComm.ExecuteScalarAsync());
            }
        }

        // Get from DB a list of commands that are similar to the one provided
        public static async Task<Dictionary<String, String>> getCommsList(MySqlConnection _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT Comm, Data FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@Comm", $"%{ _Comm }%");
                Dictionary<String, String> commsList = new Dictionary<String, String>();
                using (MySqlDataReader reader = await mySQLComm.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) 
                        commsList.Add(reader[0].ToString(), reader[1].ToString());
                } return commsList;
            }
        }

        /* ---------------------------------------------------------------------------- */

        // Add a new command on DB for the current server
        public static async Task newCommand(MySqlConnection _MyConn, ulong _ServerID, ulong _ChannelID, 
            String _Comm, String _Data, ulong _UserID) {
            String sqlQuery = "INSERT INTO tbl_Comms (ServerID, ChannelID, Comm, Data, UserID) " +
                "VALUES (@ServerID, @ChannelID, @Comm, @Data, @UserID)";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@ChannelID", _ChannelID);
                mySQLComm.Parameters.AddWithValue("@Comm", _Comm);
                mySQLComm.Parameters.AddWithValue("@Data", _Data);
                mySQLComm.Parameters.AddWithValue("@UserID", _UserID);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }

        // Change data from an existent command on DB for the current server
        public static async Task changeCommand(MySqlConnection _MyConn, ulong _ServerID, String _Comm, String _Data, ulong _UserID) {
            String sqlQuery = "UPDATE tbl_Comms SET Data=@Data, UserID=@UserID WHERE ServerID=@ServerID AND Comm=@Comm";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@Comm", _Comm);
                mySQLComm.Parameters.AddWithValue("@Data", _Data);
                mySQLComm.Parameters.AddWithValue("@UserID", _UserID);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }

        // Delete an existent command on DB for the current server
        public static async Task deleteCommand(MySqlConnection _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "DELETE FROM tbl_Comms WHERE ServerID=@ServerID AND Comm=@Comm";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@Comm", _Comm);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }
    }
}