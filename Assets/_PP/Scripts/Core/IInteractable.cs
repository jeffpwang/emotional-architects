using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    // TODO: implement this interface for all members who can be interacted with
    public interface IInteractable
    {
        enum Interaction
        {
            SwipeLeft, 
            SwipeRight, 
            Pinch, 
            ThumbsUp, 
            ThumbsDown
        }
        
        Interaction interaction { get; }
        void OnInteract();
        
        // TODO: is this the right way to do this?
        bool moveToNext { get; }
        Action completeCallback { get; set; }
        void SetCompletionCallback(Action callback);
    }
}
