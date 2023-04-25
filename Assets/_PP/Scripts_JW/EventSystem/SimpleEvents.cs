using UnityEngine;

/*
To use simple events do not modify this file. Instead, create a new script in your
project and paste the following into it:

using UnityEngine;
namespace DeltaReality.Utils
{
    public partial class Events : MonoBehaviour
    {
        #if DeltaReality_CUSTOM_SIMPLE_EVENTS
        public enum Simple
        {
            Notify
        }
        #endif
    }
}

Then enable simple events in the menu, DeltaReality->Events->Simple Events
*/

namespace Meta.PP
{
    public partial class Events : MonoBehaviour
    {
        #if !DeltaReality_CUSTOM_SIMPLE_EVENTS
        //Simple events enum, for events taht don't have any parameters
        public enum Simple
        {
            Notify
        }
        #endif
    }
}