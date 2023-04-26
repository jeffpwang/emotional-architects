using System;
using UnityEngine;

namespace Meta.PP
{
    public class ExampleUsage : MonoBehaviour
    {
        public bool showMenu = false;
        private void OnEnable()
        {
            Events.AddListener<ExampleEvent>(Call);
        }

        private void OnDisable()
        {
            Events.RemoveListener<ExampleEvent>(Call);
        }

        private void Start()
        {
            Events.Raise(new ExampleEvent(true));
        }

        private void Call(ExampleEvent e)
        {
            Debug.Log("CALLING EVENT");
        }
    }
}

