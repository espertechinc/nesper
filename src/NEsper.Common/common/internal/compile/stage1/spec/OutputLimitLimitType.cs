///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Enum for describing the type of output limit within an interval.</summary>
    public enum OutputLimitLimitType
    {
        /// <summary>
        /// Output first event, relative to the output batch.
        /// </summary>
        FIRST,

        /// <summary>
        /// Output last event, relative to the output batch.
        /// </summary>
        LAST,

        /// <summary>
        /// The ALL keyword has been explicitly specified: Output all events,
        /// relative to the output batch.
        /// <para/>
        /// In the fully-grouped and aggregated case, the explicit ALL outputs one row for each group.
        /// </summary>
        ALL,

        /// <summary>
        /// The ALL keyword has not been explicitly specified: Output all events, relative
        /// to the output batch.
        /// <para/>
        /// In the fully-grouped and aggregated case, the
        /// default ALL outputs all events of the batch row-by-row, multiple per group.
        /// </summary>
        DEFAULT,

        /// <summary>
        /// Output a snapshot of the current state, relative to the full historical state of a statement.
        /// </summary>
        SNAPSHOT
    }
} // End of namespace