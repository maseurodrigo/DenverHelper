using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;

namespace DenverHelper.Data.MySQL
{
    public class MySQLConnect 
    {
        private MySqlConnectionStringBuilder myConnBuilder { get; set; }
        public MySqlConnection myConn { get; private set; }
        public MySQLConnect(BotData _BotData) {
            myConnBuilder = new MySqlConnectionStringBuilder {
                Server = _BotData.MySQLConnect.Server,
                UserID = _BotData.MySQLConnect.UserName,
                Password = _BotData.MySQLConnect.UserPWord,
                Database = _BotData.MySQLConnect.Database
            };
        }

        // Create a new MySQL connection
        public async Task newConnection() {
            try {
                myConn = new MySqlConnection(myConnBuilder.ConnectionString);
                await myConn.OpenAsync();
            } catch (Exception excep) {
                if(excep is MySqlException) Console.WriteLine(excep.Message);
            }
        }

        // Close current MySQL connection
        public async Task closeConnection() {
            if(myConn.State.Equals(ConnectionState.Open)) await myConn.CloseAsync();
        }

        // Operation to manage parameterized data from DB
        public async Task manageData(String strSQL, List<MySqlParameter> listParams) {
            // If conn. its closed make a new connection
            if (myConn is null || myConn.State.Equals(ConnectionState.Closed)) await newConnection();
            using (MySqlCommand mySQLComm = new MySqlCommand(strSQL, myConn)) {
                // Loop through all sqlparams
                foreach (MySqlParameter param in listParams) mySQLComm.Parameters.Add(param);
                await mySQLComm.ExecuteNonQueryAsync();
            }
        }

        // Operation to get parameterized data from DB (ExecuteReaderAsync)
        public async Task<String> getReaderData(String strSQL, List<MySqlParameter> listParams) {
            // If conn. its closed make a new connection
            if (myConn is null || myConn.State.Equals(ConnectionState.Closed)) await newConnection();
            using (MySqlCommand mySQLComm = new MySqlCommand(strSQL, myConn)) {
                // Loop through all sqlparams
                foreach (MySqlParameter param in listParams) mySQLComm.Parameters.Add(param);
                using (MySqlDataReader reader = await mySQLComm.ExecuteReaderAsync()) {
                    await reader.ReadAsync();
                    return reader.GetString(0);
                }
            }
        }

        public async Task<Dictionary<String, String>> getReaderListData(String strSQL, List<MySqlParameter> listParams) {
            // If conn. its closed make a new connection
            if (myConn is null || myConn.State.Equals(ConnectionState.Closed)) await newConnection();
            Dictionary<String, String> commsList = new Dictionary<String, String>();
            using (MySqlCommand mySQLComm = new MySqlCommand(strSQL, myConn)) {
                // Loop through all sqlparams
                foreach (MySqlParameter param in listParams) mySQLComm.Parameters.Add(param);
                using (MySqlDataReader reader = await mySQLComm.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) commsList.Add(reader[0].ToString(), reader[1].ToString());
                    return commsList;
                }
            }
        }

        // Operation to get parameterized data from DB (ExecuteScalarAsync)
        public async Task<int> getScalarData(String strSQL, List<MySqlParameter> listParams) {
            // If conn. its closed make a new connection
            if (myConn is null || myConn.State.Equals(ConnectionState.Closed)) await newConnection();
            using (MySqlCommand mySQLComm = new MySqlCommand(strSQL, myConn)) {
                // Loop through all sqlparams
                foreach (MySqlParameter param in listParams) mySQLComm.Parameters.Add(param);
                return Convert.ToInt32(await mySQLComm.ExecuteScalarAsync());
            }
        }
    }
}