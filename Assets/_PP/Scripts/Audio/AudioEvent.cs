namespace Meta.PP
{
    public class AudioEvent : BaseEvent
    {
        public AudioTypeEnum AudioTypeEnum { get; set; }

        public AudioEvent(AudioTypeEnum audioTypeEnum)
        {
            AudioTypeEnum = audioTypeEnum;
        }
    }
}