namespace Meta.PP
{
    /// <summary>
    /// Class that has to be extended when creating new event.
    /// </summary>
    public class BaseEvent
    {
        public virtual Events.Category Category { get { return Events.Category.Default; } }
    }
}