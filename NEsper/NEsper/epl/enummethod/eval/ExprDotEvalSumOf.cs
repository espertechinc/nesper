///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.eval
{
    [Serializable]
    public class ExprDotEvalSumOf : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            return ExprDotNodeUtility.GetSingleLambdaParamEventType(enumMethodUsedName, goesToNames, inputEventType, collectionComponentType, eventAdapterService);
        }

        public override EnumEval GetEnumEval(MethodResolutionService methodResolutionService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            if (bodiesAndParameters.IsEmpty())
            {
                var aggMethodFactoryX = GetAggregatorFactory(collectionComponentType);
                TypeInfo = EPTypeHelper.SingleValue(aggMethodFactoryX.ValueType.GetBoxedType());
                return new EnumEvalSumScalar(numStreamsIncoming, aggMethodFactoryX);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var aggMethodFactory = GetAggregatorFactory(first.BodyEvaluator.ReturnType);
            var returnType = aggMethodFactory.ValueType.GetBoxedType();
            TypeInfo = EPTypeHelper.SingleValue(returnType);
            if (inputEventType == null)
            {
                return new EnumEvalSumScalarLambda(
                    first.BodyEvaluator, first.StreamCountIncoming, aggMethodFactory,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            return new EnumEvalSumEvents(first.BodyEvaluator, first.StreamCountIncoming, aggMethodFactory);
        }

        private static ExprDotEvalSumMethodFactory GetAggregatorFactory(Type evalType)
        {
            if (evalType.IsFloatingPointClass())
            {
                return new ProxyExprDotEvalSumMethodFactory
                {
                    ProcSumAggregator = () => new ExprDotEvalSumMethodDouble(),
                    ProcValueType = () => typeof(double?)
                };
            }
            else if (evalType.GetBoxedType() == typeof(decimal?))
            {
                return new ProxyExprDotEvalSumMethodFactory
                {
                    ProcSumAggregator = () => new ExprDotEvalSumMethodBigDecimal(),
                    ProcValueType = () => typeof(decimal?)
                };
            }
            else if (evalType.GetBoxedType() == typeof(long?))
            {
                return new ProxyExprDotEvalSumMethodFactory
                {
                    ProcSumAggregator = () => new ExprDotEvalSumMethodLong(),
                    ProcValueType = () => typeof(long?)
                };
            }
            else
            {
                return new ProxyExprDotEvalSumMethodFactory
                {
                    ProcSumAggregator = () => new ExprDotEvalSumMethodInteger(),
                    ProcValueType = () => typeof(int?)
                };
            }
        }
    
        private class ExprDotEvalSumMethodDouble : ExprDotEvalSumMethod
        {
            private double _sum;
            private long _numDataPoints;
    
            public void Enter(Object @object)
            {
                if (@object == null)
                {
                    return;
                }
                _numDataPoints++;
                _sum += @object.AsDouble();
            }

            public object Value
            {
                get { return _numDataPoints == 0 ? (object) null : _sum; }
            }
        }
    
        private class ExprDotEvalSumMethodBigDecimal : ExprDotEvalSumMethod
        {
            private decimal _sum;
            private long _numDataPoints;
    
            public ExprDotEvalSumMethodBigDecimal()
            {
                _sum = 0.0m;
            }
    
            public void Enter(Object @object)
            {
                if (@object == null)
                {
                    return;
                }
                _numDataPoints++;
                _sum += @object.AsDecimal();
            }

            public object Value
            {
                get { return _numDataPoints == 0 ? (object) null : _sum; }
            }
        }
    
        private class ExprDotEvalSumMethodLong : ExprDotEvalSumMethod
        {
            private long _sum;
            private long _numDataPoints;
    
            public void Enter(Object @object)
            {
                if (@object == null)
                {
                    return;
                }
                _numDataPoints++;
                _sum += @object.AsLong();
            }

            public object Value
            {
                get { return _numDataPoints == 0 ? (object) null : _sum; }
            }
        }
    
        private class ExprDotEvalSumMethodInteger : ExprDotEvalSumMethod
        {
            private int _sum;
            private long _numDataPoints;
    
            public void Enter(Object @object)
            {
                if (@object == null)
                {
                    return;
                }
                _numDataPoints++;
                _sum += @object.AsInt();
            }

            public object Value
            {
                get { return _numDataPoints == 0 ? (object) null : _sum; }
            }
        }
    }
}
