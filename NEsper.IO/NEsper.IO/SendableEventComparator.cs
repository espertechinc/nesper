using System.Collections.Generic;

namespace com.espertech.esperio
{
    /// <summary>
    /// A comparator that orders SendableEvents first on sendTime, and
    /// then on schedule slot.
    /// </summary>

    public class SendableEventComparator : IComparer<SendableEvent>
    {
        public int Compare(SendableEvent one, SendableEvent two)
        {
            if (one.SendTime < two.SendTime)
            {
                return -1;
            }
            else if (one.SendTime > two.SendTime)
            {
                return 1;
            }
            else
            {
                if (one.ScheduleSlot == two.ScheduleSlot)
                {
                    return 0;
                }
                else if (one.ScheduleSlot < two.ScheduleSlot)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}