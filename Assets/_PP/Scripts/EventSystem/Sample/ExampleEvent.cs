using Meta.PP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    public class ExampleEvent : BaseEvent
    {
        public bool ExampleParameter { get; set; }

        public ExampleEvent(bool exampleParameter)
        {
            ExampleParameter = exampleParameter;
            Debug.Log("HELLO EVENT");
        }
    }
}