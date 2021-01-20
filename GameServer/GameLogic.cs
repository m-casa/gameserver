
namespace GameServer
{
    class GameLogic
    {
        // Similar to Unity's update method
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
