///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableSerdes
    {
        public TableSerdes(
            DataInputOutputSerdeWCollation<object>[] column,
            DataInputOutputSerdeWCollation<object> aggregations)
        {
            if (column == null || aggregations == null) {
                throw new ArgumentException("Expected serdes not received");
            }

            ColumnStartingZero = column;
            Aggregations = aggregations;
        }

        public DataInputOutputSerdeWCollation<object>[] ColumnStartingZero { get; }

        public DataInputOutputSerdeWCollation<object> Aggregations { get; }
    }
} // end of namespace