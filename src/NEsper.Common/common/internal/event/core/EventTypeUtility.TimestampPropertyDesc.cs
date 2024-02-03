namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class TimestampPropertyDesc
        {
            public TimestampPropertyDesc(
                string start,
                string end)
            {
                Start = start;
                End = end;
            }

            public string Start { get; }

            public string End { get; }
        }
    }
}