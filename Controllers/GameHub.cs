using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;

public class GameHub: Hub
{
    public async Task SendMessage(string user)
    {
        await Clients.All.SendAsync("ReceiveMessage", "Message from server");
    }
}
