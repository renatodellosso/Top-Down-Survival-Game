using Assets.Src;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Assets.Src.Components.Gameplay {
    public class PlayerController : NetworkBehaviour
    {

        new Rigidbody2D rigidbody;

        //It seems this can't be a property
        readonly NetworkVariable<Player> player = new();
        public Player Player
        {
            get
            {
                return player.Value;
            }
            private set
            {
                player.Value = value;
            }
        }

        KeyCode[] userInputs = Array.Empty<KeyCode>();

        const float BASE_SPEED = 2.5f;
        
        //Camera config
        const float MOUSE_POS_WEIGHT = 0.35f, MAX_CAMERA_DIST_FROM_PLAYER = 5f;

        // Start is called before the first frame update
        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            if (IsServer)
                SetAccountIdClientRpc(SaveManager.ServerId, Utils.OnlySendRpcTo(OwnerClientId));
        }

        //OnNetworkSpawn must be overridden and public in order to run when the NetworkObject is spawned
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Utils.Log("Player spawned on network");
        }

        [ClientRpc]
        void SetAccountIdClientRpc(string serverId, ClientRpcParams clientRpcParams = default)
        {
            Utils.Log("Received RPC to load account ID");
            SetAccountIdServerRpc(SaveManager.GetServerAccountHash(serverId));
        }

        [ServerRpc]
        void SetAccountIdServerRpc(string playerId)
        {
            PlayerManager.AddPlayer(playerId, this);
        }

        public void SetPlayer(Player player)
        {
            Player = player;
        }

        void FixedUpdate()
        {
            if(IsLocalPlayer)
            {
                SendUserInputs();
                PositionCamera();
            }

            if (IsServer)
                HandleUserInputs();
        }

        void SendUserInputs()
        {
            List<KeyCode> inputs = new();

            if (Input.GetKey(KeyCode.W))
                inputs.Add(KeyCode.W);

            if (Input.GetKey(KeyCode.A))
                inputs.Add(KeyCode.A);

            if (Input.GetKey(KeyCode.S))
                inputs.Add(KeyCode.S);

            if (Input.GetKey(KeyCode.D))
                inputs.Add(KeyCode.D);

            if (inputs.Count > 0)
                SetUserInputsServerRpc(inputs.ToArray());
        }

        [ServerRpc]
        void SetUserInputsServerRpc(KeyCode[] inputs)
        {
            userInputs = inputs;
        }

        void HandleUserInputs()
        {
                Vector2 movement = Vector2.zero;

                //Handle each input
                foreach (KeyCode input in userInputs)
                {
                    switch (input)
                    {
                        case KeyCode.W:
                            movement += Vector2.up;
                            break;
                        case KeyCode.A:
                            movement += Vector2.left;
                            break;
                        case KeyCode.S:
                            movement += Vector2.down;
                            break;
                        case KeyCode.D:
                            movement += Vector2.right;
                            break;
                    }
                }

                //Adjust movement for diagonal
                if (movement.y != 0 && movement.x != 0)
                {
                    movement /= Mathf.Sqrt(2);
                }

            try
            {
                //Apply movement
                if (movement != Vector2.zero)
                {
                    print($"Player.Value: {player.Value}, Speed: {player.Value?.Speed}");
                    movement *= player.Value.Speed * BASE_SPEED * Time.deltaTime;
                    rigidbody.MovePosition(rigidbody.position + movement);
                }
            }
            catch (Exception e)
            {
                Utils.Log(e, "Error handling user inputs");
            }

            //Reset inputs
            userInputs = Array.Empty<KeyCode>();
        }

        void PositionCamera()
        {
            Camera camera = Camera.main;
            
            //Get all the positions we'll need
            Vector3 cameraPos = camera.transform.position;
            Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

            //Calculate the target position
            Vector3 targetPosition = (transform.position + mousePos * MOUSE_POS_WEIGHT) / (1+MOUSE_POS_WEIGHT);

            //Clamp the target position to within a certain distance of the player
            targetPosition = Vector3.MoveTowards(transform.position, targetPosition, MAX_CAMERA_DIST_FROM_PLAYER);

            //Lock the z-axis
            targetPosition.z = cameraPos.z; //Keep the camera's z position the same, it messes stuff up if it changes

            //Move the camera
            Camera.main.transform.position = targetPosition;

            //Mark the target position
            Utils.MarkPos(targetPosition, duration: Time.fixedDeltaTime);
        }

    }
}