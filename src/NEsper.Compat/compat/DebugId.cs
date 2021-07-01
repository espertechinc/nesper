using System.Threading;

namespace com.espertech.esper.compat
{
    public class DebugId<T>
    {
        private static long _typeId;

        /// <summary>
        /// Returns the identity counter for the type.
        /// </summary>
        public static long TypeId => _typeId;

        /// <summary>
        /// Returns a debug id for this type.
        /// </summary>
        /// <returns></returns>
        public static long NewId()
        {
            return Interlocked.Increment(ref _typeId);
        }
    }
}