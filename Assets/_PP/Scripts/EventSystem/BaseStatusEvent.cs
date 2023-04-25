namespace DeltaReality.Utils
{
    /// <summary>
    /// Base class used for events when only simple status is needed
    /// </summary>
    public class BaseStatusEvent : BaseEvent
    {
        public bool Sucess { get; protected set;}

        public BaseStatusEvent(bool sucess)
        {
            Sucess = sucess;
        }
    }
}