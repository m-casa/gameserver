
namespace GameServer
{
    class GameLogic
    {
        // Will be used similarly to Unity's update method
        public static void Update()
        {
            // For each connected client, update their instance with other players' information
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
