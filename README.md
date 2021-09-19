# DiscordDenver

  + Get current status of bot connection
    + **!connection**
  + Set new API key to be assigned to current server
    + **!addkey**
  + Delete API key thats assigned to the current server
    + **!deletekey**
  + Get the total amount of comms added to the current server
    + **!servercomms**
  + Get data associated with the given command to this server
    + **!getcomm**
  + Set a new command to be assigned to the current server
    + **!newcomm**
  + Delete command thats assigned to the current server
    + **!deletecomm**
  + Send a DM for users with a given role assigned
    + **!dmrole**
  + Send a DM to all server members
    + **!dmall**
  + Generate random data for the information provided
    + **!random** (options splitted by (',' or '|' or '/'))
  + Connect the bot to the current voice channel and start a music or playlist
    + **!play**
  + Pause current discord player
    + **!pause**
  + Resume current discord player
    + **!resume**
  + Stop discord player and finish the current queue
    + **!stop**
  + List all the tracks that are currently on queue
    + **!queue**
  + Searching for current track lyrics
    + **!lyrics**
  + Get the timezone and current weather conditions for multiple locations
    + **!city**
  + Get some informations about a given soccer team
    + **!team**
    + **!matches** (last/next)
  + Get predictions for the next scheduled matches of a particular soccer team
    + **!predict**
  + Get informations and statistics relating to the given player
    + **!player**
  + Get the price of some cryptocurrencies and their status
    + **!price**
    + **!convert**
  + Create a new poll with emojis reactions
    + **!poll** ... a(👍), b(👎), c(👌), ...
  
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
