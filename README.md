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
| **city** | **weather** | Get the timezone and current weather conditions for multiple locations |
| **matches** | | Get scheduled matches for a particular soccer team |
| **predict** | **tip** | Get predictions for the next scheduled matches of a particular soccer team |
| **goals** | **highlights** | Get the latest goals and highlights video from a match of a particular soccer team |
| **nba_team** |  | Get data from an NBA team |
| **price** |  | Get the price of some cryptocurrencies and their status |
| **poll** |  | Create a new poll with emojis reactions - **(question) / a(👍), b(👎), c(👌), ...** |
| **giveaway** |  | Create a new giveaway with custom options |
| **givejoin** | **giveawayjoin**  | Join a current giveaway on the server |
| **cgpt** | | Completions with an AI bot |

```json
{
  "DiscordBotPrefix": "!",
  "DiscordBotToken": "...",
  "ChatGPTApiKey": "...",
  "MySQLConnect": {
    "Server": "",
    "UserID": "",
    "UserPW": "",
    "Database": ""
  }
}
```
