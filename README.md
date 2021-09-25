# DenverHelper
  
| Command | Alias | Description |
| --- | --- | --- |
| **connection** | **conn** | Get current status of bot connection |
| **addkey** | **newkey** | Set new API key to be assigned to current server |
| **deletekey** | **removekey** | Delete API key thats assigned to the current server |
| **servercomms** | **comms** | Get the total amount of comms added to the current server |
| **getcomm** | **comm** | Get data associated with the given command to this server |
| **newcomm** | **addcomm** | Set a new command to be assigned to the current server |
| **deletecomm** | **delcomm** | Delete command thats assigned to the current server |
| **dmrole** |  | Send a DM for users with a given role assigned |
| **dmall** |  | Send a DM to all server members |
| **random** | **rand** | Generate random data for the information provided - **(options splitted by ',' or '/')** |
| **city** | **weather** | Get the timezone and current weather conditions for multiple locations |
| **team**/**matches** | **squad** | Get some informations about a given soccer team |
| **predict** | **tip** | Get predictions for the next scheduled matches of a particular soccer team |
| **player** |  | Get informations and statistics relating to the given player |
| **nba_team** |  | Get data from an NBA team |
| **nba_player** |  | Get informations relating to the given NBA player |
| **price**/**convert** |  | Get the price of some cryptocurrencies and their status |
| **poll** |  | Create a new poll with emojis reactions - **(question) / a(👍), b(👎), c(👌), ...** |

```json
{
  "DiscordBotPrefix": "!",
  "DiscordBotToken": "...",
  "MySQLConnect": {
    "Server": "",
    "UserID": "",
    "UserPW": "",
    "Database": ""
  }
}
```
