namespace com.espertech.esper.compat
{
    public class DebugExtensions
    {
        /// <summary>
        /// Used as an entry point for debugging equality operations.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool DebugEquals(
            object left,
            object right)
        {
            var result = Equals(left, right);
            return result;
        }
    }
}