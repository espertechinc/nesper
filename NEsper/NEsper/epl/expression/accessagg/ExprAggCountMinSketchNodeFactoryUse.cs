///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprAggCountMinSketchNodeFactoryUse : ExprAggCountMinSketchNodeFactoryBase
    {
        private readonly ExprEvaluator _addOrFrequencyEvaluator;
    
        public ExprAggCountMinSketchNodeFactoryUse(ExprAggCountMinSketchNode parent, ExprEvaluator addOrFrequencyEvaluator)
            : base(parent)
        {
            _addOrFrequencyEvaluator = addOrFrequencyEvaluator;
        }

        public override Type ResultType
        {
            get
            {
                var parent = Parent;
                if (parent.AggType == CountMinSketchAggType.ADD)
                {
                    return null;
                }
                else if (parent.AggType == CountMinSketchAggType.FREQ)
                {
                    return typeof(long?);
                }
                else if (parent.AggType == CountMinSketchAggType.TOPK)
                {
                    return typeof(CountMinSketchTopK[]);
                }
                else
                {
                    throw new UnsupportedOperationException("Unrecognized code " + parent.AggType);
                }
            }
        }

        public override AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }
    
        public override AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public override AggregationAccessor Accessor
        {
            get
            {
                var parent = Parent;
                if (parent.AggType == CountMinSketchAggType.ADD)
                {
                    // modifications handled by agent
                    return CountMinSketchAggAccessorDefault.INSTANCE;
                }
                else if (parent.AggType == CountMinSketchAggType.FREQ)
                {
                    return new CountMinSketchAggAccessorFrequency(_addOrFrequencyEvaluator);
                }
                else if (parent.AggType == CountMinSketchAggType.TOPK)
                {
                    return CountMinSketchAggAccessorTopk.INSTANCE;
                }

                throw new IllegalStateException(
                    "Aggregation accessor not available for this function '" + parent.AggregationFunctionName + "'");
            }
        }

        public override AggregationAgent AggregationStateAgent
        {
            get
            {
                var parent = Parent;
                if (parent.AggType == CountMinSketchAggType.ADD)
                {
                    if (parent.OptionalFilter == null) {
                        return new CountMinSketchAggAgentAdd(_addOrFrequencyEvaluator);
                    }

                    return new CountMinSketchAggAgentAddFilter(
                        _addOrFrequencyEvaluator, parent.OptionalFilter.ExprEvaluator);
                }

                throw new IllegalStateException(
                    "Aggregation agent not available for this function '" + parent.AggregationFunctionName + "'");
            }
        }

        public override void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            throw new IllegalStateException("Aggregation not compatible");
        }

        public ExprEvaluator AddOrFrequencyEvaluator
        {
            get { return _addOrFrequencyEvaluator; }
        }

        public override ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }
    }
}
