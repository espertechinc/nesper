///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public enum ExprTableEvalStrategyEnum
    {
        UNGROUPED_TOP,
        GROUPED_TOP,

        UNGROUPED_PLAINCOL,
        GROUPED_PLAINCOL,

        UNGROUPED_AGG_SIMPLE,
        GROUPED_AGG_SIMPLE,

        UNGROUPED_AGG_ACCESSREAD,
        GROUPED_AGG_ACCESSREAD,

        KEYS
    }
} // end of namespace