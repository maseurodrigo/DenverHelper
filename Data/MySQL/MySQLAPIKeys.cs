using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace DenverHelper.Data.MySQL
{
    public class MySQLAPIKeys 
    {
        // Check if API key already exists on DB
        public static async Task<bool> checkAPIKeyExists(MySQLConnect _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_APIKeys WHERE ServerID=@ServerID";
            List<MySqlParameter> listParams = new List<MySqlParameter>() { new MySqlParameter("@ServerID", _ServerID) };
            if (Convert.ToInt32(await _MyConn.getScalarData(sqlQuery, listParams)) > 0) return true;
            else return false;
        }

        // Get API key from DB for the current server
        public static async Task<String> getAPIKey(MySQLConnect _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT APIKey FROM tbl_APIKeys WHERE ServerID=@ServerID";
            List<MySqlParameter> listParams = new List<MySqlParameter>() { new MySqlParameter("@ServerID", _ServerID) };
            return await _MyConn.getReaderData(sqlQuery, listParams);
        }

        // Add a new API Key on DB for the current server
        public static async Task newAPIKey(MySQLConnect _MyConn, ulong _ServerID, String _APIKey, ulong _UserID) {
            String sqlQuery = "INSERT INTO tbl_APIKeys (ServerID, APIKey, UserID) VALUES (@ServerID, @APIKey, @UserID)";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@APIKey", _APIKey),
                new MySqlParameter("@UserID", _UserID)
            };
            await _MyConn.manageData(sqlQuery, listParams);
        }

        // Change data from an existent API Key on DB for the current server
        public static async Task changeAPIKey(MySQLConnect _MyConn, ulong _ServerID, String _APIKey, ulong _UserID) {
            String sqlQuery = "UPDATE tbl_APIKeys SET APIKey=@APIKey, UserID=@UserID WHERE ServerID=@ServerID";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@APIKey", _APIKey),
                new MySqlParameter("@UserID", _UserID),
                new MySqlParameter("@ServerID", _ServerID)
            };
            await _MyConn.manageData(sqlQuery, listParams);
        }

        // Delete an existent API Key on DB for the current server
        public static async Task deleteAPIKey(MySQLConnect _MyConn, ulong _ServerID) {
            String sqlQuery = "DELETE FROM tbl_APIKeys WHERE ServerID=@ServerID";
            List<MySqlParameter> listParams = new List<MySqlParameter>() { new MySqlParameter("@ServerID", _ServerID) };
            await _MyConn.manageData(sqlQuery, listParams);
        }
    }
}