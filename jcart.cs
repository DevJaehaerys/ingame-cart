using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Libraries;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("jcart", "0.1", "DevJaehaerys")]
    [Description("Cart plugin")]

    public class jcart : CovalencePlugin
    {
        private const string ApiUrl = "http://127.0.0.1:8000/api/cart/userItem/{0}";
        private const string ApiKey = "SSSS";
        private const float Timeout = 200f;

        [Command("get")]
        private void GetRequest(IPlayer player, string command, string[] args)
        {
            // Set the request headers
            Dictionary<string, string> headers = new Dictionary<string, string> { { "X-API-KEY", ApiKey } };

            // Format the API URL with the player's SteamID
            string url = string.Format(ApiUrl, player.Id);

            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (response == null || code != 200)
                {
                    Puts($"Error: {response} - Couldn't get a response from API for {player.Name}");
                    return;
                }

                List<object> commands = JsonConvert.DeserializeObject<List<object>>(response);
                if (commands.Count == 0)
                {
                    player.Reply($"Your cart is empty, {player.Name}!");
                    return;
                }
                
                foreach (object cmd in commands)
                {
                    Dictionary<string, object> commandz = JsonConvert.DeserializeObject<Dictionary<string, object>>(cmd.ToString());
                    string itemId = commandz["id"].ToString();
                    server.Command(commandz["command"].ToString(), Array.Empty<string>());

                    // Make a request to remove the item with the given ID from the API
                    string url2 = $"http://127.0.0.1:8000/api/cart/removeItem/{itemId}";
                    webrequest.Enqueue(url2, null, (code2, response2) =>
                    {
                        if (code2 != 200 || response2 == null)
                        {
                            Puts($"Couldn't remove item with ID {response2} from the database!");
                            return;
                        }
                        Puts($"Removed item with ID {itemId} from the database.");
                    }, this, RequestMethod.GET, headers, Timeout);
                }
            }, this, RequestMethod.GET, headers, Timeout);
        }
    }
}
