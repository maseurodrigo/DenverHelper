using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace DiscordDenver.Data.MySQL
{
    public class MySQLConnect 
    {
        private MySqlConnectionStringBuilder myConnBuilder { get; set; }
        public MySqlConnection myConn { get; private set; }
        public MySQLConnect(String _Server, String _UserID, String _PW, String _DB) {
            myConnBuilder = new MySqlConnectionStringBuilder {
                Server = _Server,
                UserID = _UserID,
                Password = _PW,
                Database = _DB
            };
        }

        // Create a new GCloud MySQL connection
        public async Task newConnection() {
            myConn = new MySqlConnection(myConnBuilder.ConnectionString);
            await myConn.OpenAsync();
        }
        // Resets the session state of the current open connection
        public async Task reConnect() { await myConn.ResetConnectionAsync(); }
        // Close current GCloud MySQL connection
        public async Task closeConnection() { await myConn.CloseAsync(); }
    }
}