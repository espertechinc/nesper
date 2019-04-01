///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public interface TableInstanceGrouped : TableInstance
    {
        //void HandleRowUpdated(ObjectArrayBackedEventBean row);

        ISet<object> GroupKeys { get; }
        ObjectArrayBackedEventBean GetRowForGroupKey(object groupKey);

        ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext);

        //Table Table { get; }
    }
} // end of namespace