using Assets.Src;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Assets.Src.Components.Gameplay {
    public class PlayerController : NetworkBehaviour
    {

        public static PlayerController Instance { get; private set; }

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

        const float BASE_SPEED = 2.5f;
        
        //Camera config
        const float MOUSE_POS_WEIGHT = 0.35f, MAX_CAMERA_DIST_FROM_PLAYER = 5f;

        //Tooltip stuff
        private Transform tooltipHolder;
        private TMP_Text tooltipTextElement;
        /// <summary>
        /// Set this to change the tooltip text
        /// </summary>
        public string TooltipText
        {
            get => tooltipTextElement.text;
            set => tooltipTextElement.text = value;
        }

        public static event Action OnInteract; 

        // Start is called before the first frame update
        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            if (IsServer)
                SetAccountIdClientRpc(SaveManager.ServerId, Utils.OnlySendRpcTo(OwnerClientId));

            if(IsOwner)
            {
                Instance = this;

                FindReferences();
            }
        }

        //OnNetworkSpawn must be overridden and public in order to run when the NetworkObject is spawned
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Utils.Log("Player spawned on network");
        }

        /// <summary>
        /// Finds the tooltip holder and text element
        /// </summary>
        private void FindReferences()
        {
            tooltipHolder = GameObject.FindGameObjectWithTag("TooltipHolder").transform;
            tooltipTextElement = tooltipHolder.GetComponentInChildren<TMP_Text>();
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
                HandleUserInputs();
                PositionCamera();

                if (tooltipHolder != null)
                    //Update tooltip position to stay with mouse
                    tooltipHolder.position = Input.mousePosition;
            }
        }

        void HandleUserInputs()
        {
            Vector2 movement = Vector2.zero;

            //Read movement inputs
            if(Input.GetKey(KeyCode.W))
                movement += Vector2.up;
            if (Input.GetKey(KeyCode.A))
                movement += Vector2.left;
            if (Input.GetKey(KeyCode.S))
                movement += Vector2.down;
            if (Input.GetKey(KeyCode.D))
                movement += Vector2.right;

            //Read interaction inputs
            //Mouse 0 is left click, 1 is right click
            if(Input.GetMouseButtonDown(1))
                OnInteract?.Invoke();

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
                    movement *= player.Value.Speed * BASE_SPEED * Time.deltaTime;
                    rigidbody.MovePosition(rigidbody.position + movement);
                }
            }
            catch (Exception e)
            {
                Utils.Log(e, "Error handling user inputs");
            }
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