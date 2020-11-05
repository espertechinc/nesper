///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Keywords for use in the for-clause.
    /// </summary>
    public enum ForClauseKeyword
    {
        /// <summary>
        /// Grouped delivery - listener receives invocation per group.
        /// </summary>
        GROUPED_DELIVERY,

        /// <summary>
        /// Discrete delivery - listener receives invocation per event.
        /// </summary>
        DISCRETE_DELIVERY,
    }

    public static class ForClauseKeywordExtensions
    {
        /// <summary>
        /// Returns for-keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>keyword</returns>
        public static string GetName(this ForClauseKeyword keyword)
        {
            switch (keyword)
            {
                case ForClauseKeyword.GROUPED_DELIVERY:
                    return "grouped_delivery";

                case ForClauseKeyword.DISCRETE_DELIVERY:
                    return "discrete_delivery";

                default:
                    return null;
            }
        }
    }
}