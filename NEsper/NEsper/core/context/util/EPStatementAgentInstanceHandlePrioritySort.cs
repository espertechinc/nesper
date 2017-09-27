using System;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.context.util
{
    public class EPStatementAgentInstanceHandlePrioritySort : StandardComparer<EPStatementAgentInstanceHandle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPStatementAgentInstanceHandlePrioritySort"/> class.
        /// </summary>
        public EPStatementAgentInstanceHandlePrioritySort()
            : base(GetComparer(false))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPStatementAgentInstanceHandlePrioritySort"/> class.
        /// </summary>
        /// <param name="useGreaterThanOrEqual">if set to <c>true</c> [use greater than or equal].</param>
        public EPStatementAgentInstanceHandlePrioritySort(bool useGreaterThanOrEqual)
            : base(GetComparer(useGreaterThanOrEqual))
        {
        }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <param name="useGreaterThanOrEqual">if set to <c>true</c> [use greater than or equal].</param>
        /// <returns></returns>
        public static Func<EPStatementAgentInstanceHandle, EPStatementAgentInstanceHandle, int> GetComparer(bool useGreaterThanOrEqual)
        {
            if (useGreaterThanOrEqual)
            {
                return CompareGte;
            }

            return CompareGt;
        }

        public static int CompareGt(EPStatementAgentInstanceHandle x, EPStatementAgentInstanceHandle y)
        {
            return x.Priority > y.Priority ? -1 : 1;
        }

        public static int CompareGte(EPStatementAgentInstanceHandle x, EPStatementAgentInstanceHandle y)
        {
            return x.Priority >= y.Priority ? -1 : 1;
        }
    }
}
