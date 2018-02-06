///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprAggCountMinSketchNodeFactoryState : ExprAggCountMinSketchNodeFactoryBase
    {
        private readonly AggregationStateFactoryCountMinSketch _stateFactory;

        public ExprAggCountMinSketchNodeFactoryState(AggregationStateFactoryCountMinSketch stateFactory)
            : base(stateFactory.Parent)
        {
            _stateFactory = stateFactory;
        }

        public override Type ResultType
        {
            get { return null; }
        }

        public override AggregationAccessor Accessor
        {
            get { return CountMinSketchAggAccessorDefault.INSTANCE; }
        }

        public override AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize) 
        {
            // For match-recognize we don't allow
            if (isMatchRecognize) {
                throw new IllegalStateException("Count-min-sketch is not supported for match-recognize");
            }
            return _stateFactory;
        }

        public override AggregationAgent AggregationStateAgent
        {
            get { throw new UnsupportedOperationException(); }
        }

        public override void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            var use = (ExprAggCountMinSketchNodeFactoryUse) intoTableAgg;
            var aggType = use.Parent.AggType;
            if (aggType == CountMinSketchAggType.FREQ || aggType == CountMinSketchAggType.ADD)
            {
                Type clazz = use.AddOrFrequencyEvaluator.ReturnType;
                var foundMatch = false;
                foreach (var allowed in _stateFactory.Specification.Agent.AcceptableValueTypes)
                {
                    if (TypeHelper.IsSubclassOrImplementsInterface(clazz, allowed)) 
                    {
                        foundMatch = true;
                    }
                }
                if (!foundMatch)
                {
                    throw new ExprValidationException(
                        "Mismatching parameter return type, expected any of " +
                        _stateFactory.Specification.Agent.AcceptableValueTypes.Render() + " but received " +
                        clazz.GetCleanName());
                }
            }
        }

        public override ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return null;
        }
    }
}
