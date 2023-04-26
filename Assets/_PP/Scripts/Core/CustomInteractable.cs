using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.PP
{
    public class CustomInteractable : MonoBehaviour
    {
        public enum Interaction
        {
            SwipeLeft, 
            SwipeRight, 
            Pinch, 
            ThumbsUp, 
            ThumbsDown
        }

        public Interaction interactionType;
        
        [SerializeField]
        private UnityEvent _whenActivated;
        [SerializeField]
        private UnityEvent _whenDeactivated;
        [SerializeField]
        private bool doOnce = true;
        private bool _isInteracted = false;
        
        void Start()
        {
            switch (interactionType)
            {
                case Interaction.SwipeLeft:
                    GestureManager.Instance.WhenSwipeLeft += Perform;
                    break;
                case Interaction.SwipeRight:
                    GestureManager.Instance.WhenSwipeRight += Perform;
                    break;
                case Interaction.Pinch:
                    GestureManager.Instance.WhenPinchSelect += Perform;
                    break;
                case Interaction.ThumbsUp:
                    break;
                case Interaction.ThumbsDown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Debug.Log($"[BrainInteractable] {gameObject.name} is listening...");
        }
        
        private void Perform()
        {
            if (doOnce && _isInteracted)
            {
                return;
            }

            _whenActivated?.Invoke();
            _isInteracted = true;
            Debug.Log($"[BrainInteractable] Perform action after gesture {gameObject.name}");
        }

        private void OnDestroy()
        {
            switch (interactionType)
            {
                case Interaction.SwipeLeft:
                    GestureManager.Instance.WhenSwipeLeft -= Perform;
                    break;
                case Interaction.SwipeRight:
                    GestureManager.Instance.WhenSwipeRight -= Perform;
                    break;
                case Interaction.Pinch:
                    GestureManager.Instance.WhenPinchSelect -= Perform;
                    break;
                case Interaction.ThumbsUp:
                    break;
                case Interaction.ThumbsDown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }

}