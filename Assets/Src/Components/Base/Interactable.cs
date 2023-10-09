using Assets.Src.Components.Gameplay;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Src.Components.Base
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public abstract class Interactable : NetworkBehaviour
    {

        

        private bool isHovered;

        //The Range(min, max) attribute adds a slider to the inspector
        [SerializeField, Range(1f, 10f)] private float interactionRange = 2.5f;

        protected abstract string GetTooltipVerb();

        public override void OnNetworkSpawn()
        {
            PlayerController.OnInteract += OnInteractStarted;
        }

        private void OnMouseEnter()
        {
            isHovered = true;
        }

        private void OnMouseExit()
        {
            PlayerController.Instance.TooltipText = "";

            isHovered = false;
        }

        private void OnMouseOver()
        {
            if (Vector2.Distance(PlayerController.Instance.transform.position, transform.position) < interactionRange)
                //TODO: change this to update the keybind based on what controller is being used
                PlayerController.Instance.TooltipText = $"[RMB] to {GetTooltipVerb()}";
            else
                PlayerController.Instance.TooltipText = "";
        }

        private void OnInteractStarted()
        {
            if (isHovered && Vector2.Distance(PlayerController.Instance.transform.position, transform.position) < interactionRange)
                OnInteract();
        }

        protected abstract void OnInteract();

    }
}
