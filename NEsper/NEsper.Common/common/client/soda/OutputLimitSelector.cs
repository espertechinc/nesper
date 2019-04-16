///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Selector for use in output rate limiting.
    /// </summary>
    [Serializable]
    public enum OutputLimitSelector
    {
        /// <summary>
        /// Output first event of last interval.
        /// </summary>
        FIRST,

        /// <summary>
        /// Output last event of last interval.
        /// </summary>
        LAST,

        /// <summary>
        /// Output all events of last interval.   For group-by statements, output all groups
        /// regardless whether the group changed between the last output.
        /// </summary>
        ALL,

        /// <summary>
        /// Output all events as a snapshot considering the current state regardless of interval.
        /// </summary>
        SNAPSHOT,

        /// <summary>
        /// Output all events of last interval.
        /// </summary>
        DEFAULT
    }

    public static class OutputLimitSelectorExtensions
    {
        public static string GetText(this OutputLimitSelector @enum)
        {
            switch (@enum) {
                case OutputLimitSelector.FIRST:
                    return "first";
                case OutputLimitSelector.LAST:
                    return "last";
                case OutputLimitSelector.ALL:
                    return "all";
                case OutputLimitSelector.SNAPSHOT:
                    return "snapshot";
                case OutputLimitSelector.DEFAULT:
                    return "default";
            }

            throw new ArgumentException();
        }
    }
} // End of namespace