using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace DenverHelper.Data.MySQL
{
    public class MySQLComms 
    {
        // Check if discord bot custom command already exists on DB
        public static async Task<bool> checkCommExists(MySQLConnect _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@Comm", _Comm)
            };
            if (Convert.ToInt32(await _MyConn.getScalarData(sqlQuery, listParams)) > 0) return true;
            else return false;
        }

        // Get command data from DB for the current server
        public static async Task<String> getCommData(MySQLConnect _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT Data FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@Comm", _Comm)
            };
            return await _MyConn.getReaderData(sqlQuery, listParams);
        }

        // Get the amount of commands on DB for the current server
        public static async Task<int> getTotalComms(MySQLConnect _MyConn, ulong _ServerID) {
            String sqlQuery = "SELECT COUNT(*) FROM tbl_Comms WHERE ServerID=@ServerID";
            List<MySqlParameter> listParams = new List<MySqlParameter>() { new MySqlParameter("@ServerID", _ServerID) };
            return await _MyConn.getScalarData(sqlQuery, listParams);
        }

        // Get from DB a list of commands that are similar to the one provided
        public static async Task<Dictionary<String, String>> getCommsList(MySQLConnect _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "SELECT Comm, Data FROM tbl_Comms WHERE ServerID=@ServerID AND Comm LIKE @Comm";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@Comm", $"%{ _Comm }%")
            };
            return await _MyConn.getReaderListData(sqlQuery, listParams);
        }

        // Add a new command on DB for the current server
        public static async Task newCommand(MySQLConnect _MyConn, ulong _ServerID, ulong _ChannelID, 
            String _Comm, String _Data, ulong _UserID) {
            String sqlQuery = "INSERT INTO tbl_Comms (ServerID, ChannelID, Comm, Data, UserID) " +
                "VALUES (@ServerID, @ChannelID, @Comm, @Data, @UserID)";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@ChannelID", _ChannelID),
                new MySqlParameter("@Comm", _Comm),
                new MySqlParameter("@Data", _Data),
                new MySqlParameter("@UserID", _UserID)
            }; 
            await _MyConn.manageData(sqlQuery, listParams);
        }

        // Change data from an existent command on DB for the current server
        public static async Task changeCommand(MySQLConnect _MyConn, ulong _ServerID, String _Comm, String _Data, ulong _UserID) {
            String sqlQuery = "UPDATE tbl_Comms SET Data=@Data, UserID=@UserID WHERE ServerID=@ServerID AND Comm=@Comm";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@Comm", _Comm),
                new MySqlParameter("@Data", _Data),
                new MySqlParameter("@UserID", _UserID)
            };
            await _MyConn.manageData(sqlQuery, listParams);
        }

        // Delete an existent command on DB for the current server
        public static async Task deleteCommand(MySQLConnect _MyConn, ulong _ServerID, String _Comm) {
            String sqlQuery = "DELETE FROM tbl_Comms WHERE ServerID=@ServerID AND Comm=@Comm";
            List<MySqlParameter> listParams = new List<MySqlParameter>() {
                new MySqlParameter("@ServerID", _ServerID),
                new MySqlParameter("@Comm", _Comm)
            };
            await _MyConn.manageData(sqlQuery, listParams);
        }
    }
}