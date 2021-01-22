using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        public string username;

        public Vector3 position;
        public Vector3 moveDirection;
        public Quaternion rotation;

        // If moving the player by server, divide the desired speed by the tick rate
        // Dividing by the tick rate has the same effect as multiplying speed by Time.deltaTime
        // private float moveSpeed = 5f / Constants.TICKS_PER_SEC;

        private float[] inputs;

        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new float[3];
        }

        // Will be used similarly to Unity's update method 
        public void Update()
        {
            Vector3 _moveDirection = Vector3.Zero;
            if (inputs[0] != 0)
            {
                _moveDirection.X = inputs[0];
            }
            if (inputs[1] != 0)
            {
                _moveDirection.Y = inputs[1];
            }
            if (inputs[2] != 0)
            {
                _moveDirection.Z = inputs[2];
            }

            Move(_moveDirection);
        }

        // Sends this player's inputs to all clients
        private void Move(Vector3 _moveDirection)
        {
            moveDirection = _moveDirection;

            ServerSend.PlayerInput(this);
            ServerSend.PlayerRotation(this);
        }

        // Stores this player's inputs to the server
        public void SetInput(float[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
