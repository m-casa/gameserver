
namespace GameServer
{
    class GameLogic
    {
        // Will be used similarly to Unity's update method
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
