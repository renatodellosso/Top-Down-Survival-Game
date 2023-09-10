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

        public NetworkVariable<Player> Player { get; protected set; } = new();

        KeyCode[] userInputs = Array.Empty<KeyCode>();

        const float BASE_SPEED = 2.5f;
        
        //Camera config
        const float MOUSE_POS_WEIGHT = 0.35f, MAX_CAMERA_DIST_FROM_PLAYER = 5f;

        // Start is called before the first frame update
        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        //OnNetworkSpawn must be overridden and public in order to run when the NetworkObject is spawned
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Utils.Log("Player spawned on network");

            if (IsServer)
                SetAccountIdClientRpc(SaveManager.ServerId);
        }

        [ClientRpc]
        void SetAccountIdClientRpc(string serverId)
        {
            SetAccountIdServerRpc(SaveManager.GetServerAccountHash(serverId));
        }

        [ServerRpc]
        void SetAccountIdServerRpc(string playerId)
        {
            PlayerManager.AddPlayer(playerId, this);
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

            //Apply movement
            if (movement != Vector2.zero)
            {
                movement *= Player.Value.Speed * BASE_SPEED * Time.deltaTime;
                rigidbody.MovePosition(rigidbody.position + movement);
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