using Assets.Src;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    new Rigidbody2D rigidbody;

    public NetworkVariable<Player> Player { get; protected set; } = new();

    KeyCode[] userInputs = Array.Empty<KeyCode>();

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
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

        if (IsServer)
            HandleUserInputs();
    }

    [ServerRpc]
    void SetUserInputsServerRpc(KeyCode[] inputs)
    {
        print($"Received inputs from client: {string.Join(", ", inputs)}");
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
        if(movement.y != 0 && movement.x != 0)
        {
            movement /= Mathf.Sqrt(2);
        }

        //Apply movement
        if(movement != Vector2.zero)
            rigidbody.velocity = Player.Value.Speed * Time.deltaTime * movement;

        //Reset inputs
        userInputs = Array.Empty<KeyCode>();
    }
}
