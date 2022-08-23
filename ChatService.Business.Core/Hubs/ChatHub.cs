using ChatService.Domain.Data.Hub;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Business.Core.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string botUser;
        private readonly IDictionary<string, UserConnection> connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            botUser = "SerberChat Bot";
            this.connections = connections;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            connections[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", botUser,
                $"{userConnection.User} has joined {userConnection.Room}");

            await SendConnectedUsers(userConnection.Room);
        }

        public override async Task<Task> OnDisconnectedAsync(Exception exception)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                connections.Remove(Context.ConnectionId);
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", botUser, 
                    $"{userConnection.User} has left");

                await SendConnectedUsers(userConnection.Room);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }

        public async Task SendConnectedUsers(string room)
        {
            var users = connections.Values.Where(c => c.Room == room).Select(c=>c.User);
            await Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
