using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
    public class EPStatementHandlePrioritySort : StandardComparer<EPStatementHandle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPStatementHandlePrioritySort"/> class.
        /// </summary>
        public EPStatementHandlePrioritySort()
            : base(GetComparer(false))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPStatementHandlePrioritySort"/> class.
        /// </summary>
        /// <param name="useGreaterThanOrEqual">if set to <c>true</c> [use greater than or equal].</param>
        public EPStatementHandlePrioritySort(bool useGreaterThanOrEqual)
            : base(GetComparer(useGreaterThanOrEqual))
        {
        }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <param name="useGreaterThanOrEqual">if set to <c>true</c> [use greater than or equal].</param>
        /// <returns></returns>
        public static Func<EPStatementHandle, EPStatementHandle, int> GetComparer(bool useGreaterThanOrEqual)
        {
            if (useGreaterThanOrEqual)
            {
                return CompareGte;
            }

            return CompareGt;
        }

        public static int CompareGt(EPStatementHandle x, EPStatementHandle y)
        {
            return x.Priority > y.Priority ? -1 : 1;
        }

        public static int CompareGte(EPStatementHandle x, EPStatementHandle y)
        {
            return x.Priority >= y.Priority ? -1 : 1;
        }
    }
}
