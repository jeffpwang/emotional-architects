using UnityEngine;

/*
To use event categories do not modify this file. Instead, create a new script in your
project and paste the following into it:

using UnityEngine;
namespace DeltaReality.Utils
{
    public partial class Events : MonoBehaviour
    {
        #if !DeltaReality_CUSTOM_EVENT_CATEGORIES
        public enum Category
        {
            Default
        }
        #endif
    }
}

Then enable event categories in the menu, DeltaReality->Events->Event Categories
*/

namespace DeltaReality.Utils
{
    public partial class Events : MonoBehaviour
    {
        #if !DeltaReality_CUSTOM_EVENT_CATEGORIES
        //Event categories, for projects where a large number of events causes a performance bottleneck in the dictionary search
        public enum Category
        {
            Default
        }
        #endif
    }
}