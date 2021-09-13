using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace DiscordDenver.Data.MySQL
{
    public class MySQLAPIKeys 
    {
        // Check if API key already exists on DB
        public static async Task<bool> checkAPIKeyExists(MySqlConnection _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_APIKeys WHERE ServerID=@ServerID";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                int result = Convert.ToInt32(await mySQLComm.ExecuteScalarAsync());
                if (result > 0) return true;
                else return false;
            }
        }

        // Get API key from DB for the current server
        public static async Task<String> getAPIKey(MySqlConnection _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT APIKey FROM tbl_APIKeys WHERE ServerID=@ServerID";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                using (MySqlDataReader reader = await mySQLComm.ExecuteReaderAsync()) {
                    await reader.ReadAsync();
                    return reader.GetString(0);
                }
            }
        }

        /* ---------------------------------------------------------------------------- */

        // Add a new API Key on DB for the current server
        public static async Task newAPIKey(MySqlConnection _MyConn, ulong _ServerID, String _APIKey, ulong _UserID) {
            String sqlQuery = "INSERT INTO tbl_APIKeys (ServerID, APIKey, UserID) VALUES (@ServerID, @APIKey, @UserID)";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                mySQLComm.Parameters.AddWithValue("@APIKey", _APIKey);
                mySQLComm.Parameters.AddWithValue("@UserID", _UserID);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }

        // Change data from an existent API Key on DB for the current server
        public static async Task changeAPIKey(MySqlConnection _MyConn, ulong _ServerID, String _APIKey, ulong _UserID) {
            String sqlQuery = "UPDATE tbl_APIKeys SET APIKey=@APIKey, UserID=@UserID WHERE ServerID=@ServerID";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@APIKey", _APIKey);
                mySQLComm.Parameters.AddWithValue("@UserID", _UserID);
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }

        // Delete an existent API Key on DB for the current server
        public static async Task deleteAPIKey(MySqlConnection _MyConn, ulong _ServerID) {
            String sqlQuery = "DELETE FROM tbl_APIKeys WHERE ServerID=@ServerID";
            using (MySqlCommand mySQLComm = new MySqlCommand(sqlQuery, _MyConn)) {
                mySQLComm.Parameters.AddWithValue("@ServerID", _ServerID);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }
    }
}