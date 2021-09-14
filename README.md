# DiscordDenver

  + Get current status of bot connection
    + **!connection**
  + Set new API key to be assigned to current server
    + **!addkey**
  + Delete API key thats assigned to the current server
    + **!deletekey**
  + Get the total amount of comms added to the current server
    + **!servercomms**
  + Get a list of similar comms found in the DB for the current server
    + **!checkcomm**
  + Get data associated with the given command to this server
    + **!getcomm**
  + Set a new command to be assigned to the current server
    + **!newcomm**
  + Delete command thats assigned to the current server
    + **!deletecomm**
  + Send a DM to a unique server member
    + **!dmuser**
  + Send a DM for users with a given role assigned
    + **!dmrole**
  + Send a DM to all server members
    + **!dmall**
  + Generate random data for the information provided
    + **!random**
  + Get the timezone and current weather conditions for multiple locations
    + **!city**
  + Get some informations about a given football team
    + **!team**
    + **!matches** (last/next)
  + Get predictions for the next scheduled matches of a particular football team
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
