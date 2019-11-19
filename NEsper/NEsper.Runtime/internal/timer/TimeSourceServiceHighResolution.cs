using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.timer
{
    /// <summary>
    /// Allow for different strategies for getting VM (wall clock) time.
    /// </summary>
    public class TimeSourceServiceHighResolution : TimeSourceService
    {
        private readonly HighResolutionTimeProvider _highResolutionTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSourceServiceHighResolution"/> class.
        /// </summary>
        public TimeSourceServiceHighResolution()
        {
            _highResolutionTimeProvider = HighResolutionTimeProvider.Instance;
        }

        /// <summary>
        /// Returns time in millis.
        /// </summary>
        public long TimeMillis => _highResolutionTimeProvider.CurrentTime;
    }
}