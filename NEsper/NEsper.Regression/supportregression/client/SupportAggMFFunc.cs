///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.client
{
    public enum SupportAggMFFunc
    {
        SINGLE_EVENT_1,
        SINGLE_EVENT_2,
        ENUM_EVENT,
        COLL_SCALAR,
        ARR_SCALAR,
        SCALAR
    }

    public static class SupportAggMFFuncExtensions
    {
        public static AggregationAccessor GetAccessor(this SupportAggMFFunc value)
        {
            switch (value)
            {
                case SupportAggMFFunc.SINGLE_EVENT_1:
                    return null;
                case SupportAggMFFunc.SINGLE_EVENT_2:
                    return null;
                case SupportAggMFFunc.ENUM_EVENT:
                    return GetAccessor(typeof (SupportAggMFAccessorEnumerableEvents));
                case SupportAggMFFunc.COLL_SCALAR:
                    return GetAccessor(typeof (SupportAggMFAccessorCollScalar));
                case SupportAggMFFunc.ARR_SCALAR:
                    return GetAccessor(typeof (SupportAggMFAccessorArrScalar));
                case SupportAggMFFunc.SCALAR:
                    return GetAccessor(typeof(SupportAggMFAccessorPlainScalar));
            }

            throw new ArgumentException();
        }
    
        public static String GetName(this SupportAggMFFunc value) {
            switch (value)
            {
                case SupportAggMFFunc.SINGLE_EVENT_1:
                    return "se1";
                case SupportAggMFFunc.SINGLE_EVENT_2:
                    return "se2";
                case SupportAggMFFunc.ENUM_EVENT:
                    return "ee";
                case SupportAggMFFunc.COLL_SCALAR:
                    return "sc";
                case SupportAggMFFunc.ARR_SCALAR:
                    return "sa";
                case SupportAggMFFunc.SCALAR:
                    return "ss";
            }

            throw new ArgumentException();
        }
    
        public static bool IsSingleEvent(String functionName) {
            return SupportAggMFFunc.SINGLE_EVENT_1.GetName().Equals(functionName) ||
                   SupportAggMFFunc.SINGLE_EVENT_2.GetName().Equals(functionName);
        }
    
        public static String[] GetFunctionNames()
        {
            return EnumHelper.GetValues<SupportAggMFFunc>().Select(GetName).ToArray();
        }
    
        public static SupportAggMFFunc FromFunctionName(String functionName) {
            foreach (SupportAggMFFunc func in EnumHelper.GetValues<SupportAggMFFunc>()) {
                if (func.GetName().Equals(functionName)) {
                    return func;
                }
            }
            throw new Exception("Unrecognized function name '" + functionName + "'");
        }
    
        private static AggregationAccessor GetAccessor(Type clazz) {
            try
            {
                return (AggregationAccessor) Activator.CreateInstance(clazz);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to instantiate: " + e.Message, e);
            }
        }
    
        public static EPType GetReturnType(this SupportAggMFFunc value, EventType eventType, ExprNode[] parameters) {
            if (value == SupportAggMFFunc.SCALAR) {
                return EPTypeHelper.SingleValue(parameters[0].ExprEvaluator.ReturnType);
            }
            if (value == SupportAggMFFunc.ENUM_EVENT) {
                return EPTypeHelper.CollectionOfEvents(eventType);
            }
            if (value == SupportAggMFFunc.COLL_SCALAR) {
                return EPTypeHelper.CollectionOfSingleValue(parameters[0].ExprEvaluator.ReturnType);
            }
            if (value == SupportAggMFFunc.ARR_SCALAR) {
                return EPTypeHelper.Array(parameters[0].ExprEvaluator.ReturnType);
            }
            if (value == SupportAggMFFunc.SINGLE_EVENT_1 || value == SupportAggMFFunc.SINGLE_EVENT_2) {
                return EPTypeHelper.SingleEvent(eventType);
            }
            throw new ArgumentException("Return type not supported for " + value);
        }
    
        public static PlugInAggregationMultiFunctionStateFactory GetStateFactory(this SupportAggMFFunc value, PlugInAggregationMultiFunctionValidationContext validationContext)
        {
            if (value == SupportAggMFFunc.SCALAR) {
                if (validationContext.ParameterExpressions.Length != 1) {
                    throw new ArgumentException("Function '" + validationContext.FunctionName + "' requires 1 parameter");
                }
                var evaluator = validationContext.ParameterExpressions[0].ExprEvaluator;
                return new SupportAggMFStatePlainScalarFactory(evaluator);
            }
            if (value == SupportAggMFFunc.ARR_SCALAR || value == SupportAggMFFunc.COLL_SCALAR) {
                if (validationContext.ParameterExpressions.Length != 1)
                {
                    throw new ArgumentException("Function '" + validationContext.FunctionName + "' requires 1 parameter");
                }
                var evaluator = validationContext.ParameterExpressions[0].ExprEvaluator;
                return new SupportAggMFStateArrayCollScalarFactory(evaluator);
            }
            if (value == SupportAggMFFunc.ENUM_EVENT) {
                return new ProxyPlugInAggregationMultiFunctionStateFactory
                {
                    ProcMakeAggregationState = stateContext => new SupportAggMFStateEnumerableEvents()
                };
            }
            throw new ArgumentException("Return type not supported for " + value);
        }
    }
}
