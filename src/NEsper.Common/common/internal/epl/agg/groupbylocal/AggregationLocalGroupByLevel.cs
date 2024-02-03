///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;


namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByLevel
    {
        private readonly AggregationRowFactory rowFactory;
        private readonly DataInputOutputSerde rowSerde;
        private readonly Type[] groupKeyTypes;
        private readonly ExprEvaluator groupKeyEval;
        private readonly bool isDefaultLevel;
        private readonly DataInputOutputSerde keySerde;

        public AggregationLocalGroupByLevel(
            AggregationRowFactory rowFactory,
            DataInputOutputSerde rowSerde,
            Type[] groupKeyTypes,
            ExprEvaluator groupKeyEval,
            bool isDefaultLevel,
            DataInputOutputSerde keySerde)
        {
            this.rowFactory = rowFactory;
            this.rowSerde = rowSerde;
            this.groupKeyTypes = groupKeyTypes;
            this.groupKeyEval = groupKeyEval;
            this.isDefaultLevel = isDefaultLevel;
            this.keySerde = keySerde;
        }

        public bool IsDefaultLevel => isDefaultLevel;

        public AggregationRowFactory RowFactory => rowFactory;

        public DataInputOutputSerde RowSerde => rowSerde;

        public Type[] GroupKeyTypes => groupKeyTypes;

        public ExprEvaluator GroupKeyEval => groupKeyEval;

        public DataInputOutputSerde KeySerde => keySerde;
    }
} // end of namespace