using com.espertech.esper.compat;

namespace com.espertech.esper.timer
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
        public long GetTimeMillis()
        {
            return _highResolutionTimeProvider.CurrentTime;
        }
    }
}
