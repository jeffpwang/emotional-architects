namespace Meta.PP
{
    public class AudioVisualizerEvent : BaseEvent
    {
        public string Text;
        public AudioVisualizerEvent(string text)
        {
            Text = text;
        }
    }
}