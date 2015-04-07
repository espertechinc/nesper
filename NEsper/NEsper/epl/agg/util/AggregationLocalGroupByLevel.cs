///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.util
{
    public class AggregationLocalGroupByLevel
    {
        public AggregationLocalGroupByLevel(
            ExprEvaluator[] methodEvaluators,
            AggregationMethodFactory[] methodFactories,
            AggregationStateFactory[] stateFactories,
            ExprEvaluator[] partitionEvaluators,
            bool defaultLevel)
        {
            MethodEvaluators = methodEvaluators;
            MethodFactories = methodFactories;
            StateFactories = stateFactories;
            PartitionEvaluators = partitionEvaluators;
            IsDefaultLevel = defaultLevel;
        }

        public ExprEvaluator[] MethodEvaluators { get; private set; }

        public AggregationMethodFactory[] MethodFactories { get; private set; }

        public AggregationStateFactory[] StateFactories { get; private set; }

        public ExprEvaluator[] PartitionEvaluators { get; private set; }

        public bool IsDefaultLevel { get; private set; }
    }
} // end of namespace