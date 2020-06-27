///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByLevel
    {
        public AggregationLocalGroupByLevel(
            AggregationRowFactory rowFactory,
            DataInputOutputSerdeWCollation<AggregationRow> rowSerde,
            Type[] groupKeyTypes,
            ExprEvaluator groupKeyEval,
            bool isDefaultLevel,
            DataInputOutputSerde keySerde)
        {
            RowFactory = rowFactory;
            RowSerde = rowSerde;
            GroupKeyTypes = groupKeyTypes;
            GroupKeyEval = groupKeyEval;
            IsDefaultLevel = isDefaultLevel;
            KeySerde = keySerde;
        }

        public AggregationRowFactory RowFactory { get; }

        public DataInputOutputSerdeWCollation<AggregationRow> RowSerde { get; }

        public Type[] GroupKeyTypes { get; }

        public ExprEvaluator GroupKeyEval { get; }

        public bool IsDefaultLevel { get; }
        
        public DataInputOutputSerde KeySerde { get; }
    }
} // end of namespace