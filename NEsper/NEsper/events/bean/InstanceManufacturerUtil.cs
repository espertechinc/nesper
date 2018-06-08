///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;
using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    public class InstanceManufacturerUtil
    {

        public static Pair<FastConstructor, ExprEvaluator[]> GetManufacturer(Type targetClass, EngineImportService engineImportService, ExprEvaluator[] exprEvaluators, object[] expressionReturnTypes)
        {
            var ctorTypes = new Type[expressionReturnTypes.Length];
            var evaluators = new ExprEvaluator[exprEvaluators.Length];

            for (var i = 0; i < expressionReturnTypes.Length; i++)
            {
                var columnType = expressionReturnTypes[i];

                if (columnType is Type || columnType == null)
                {
                    ctorTypes[i] = (Type)expressionReturnTypes[i];
                    evaluators[i] = exprEvaluators[i];
                    continue;
                }

                if (columnType is EventType)
                {
                    var columnEventType = (EventType)columnType;
                    var returnType = columnEventType.UnderlyingType;
                    var inner = exprEvaluators[i];
                    evaluators[i] = new ProxyExprEvaluator
                    {
                        ProcEvaluate = evaluateParams =>
                        {
                            var theEvent = (EventBean)inner.Evaluate(evaluateParams);
                            if (theEvent != null)
                            {
                                return theEvent.Underlying;
                            }
                            return null;
                        },

                        ProcReturnType = () =>
                        {
                            return returnType;
                        },

                    };
                    ctorTypes[i] = returnType;
                    continue;
                }

                // handle case where the select-clause contains an fragment array
                if (columnType is EventType[])
                {
                    var columnEventType = ((EventType[])columnType)[0];
                    var componentReturnType = columnEventType.UnderlyingType;

                    var inner = exprEvaluators[i];
                    evaluators[i] = new ProxyExprEvaluator
                    {
                        ProcEvaluate = evaluateParams =>
                        {
                            var result = inner.Evaluate(evaluateParams);
                            if (!(result is EventBean[]))
                            {
                                return null;
                            }
                            var events = (EventBean[])result;
                            var values = Array.CreateInstance(componentReturnType, events.Length);
                            for (var jj = 0; jj < events.Length; jj++)
                            {
                                values.SetValue(events[jj].Underlying, jj);
                            }
                            return values;
                        },

                        ProcReturnType = () =>
                        {
                            return componentReturnType;
                        },

                    };
                    continue;
                }

                var message = "Invalid assignment of expression " + i + " returning type '" + columnType +
                        "', column and parameter types mismatch";
                throw new ExprValidationException(message);
            }
            try
            {
                var ctor = engineImportService.ResolveCtor(targetClass, ctorTypes);
                var fastClass = FastClass.Create(targetClass);
                return new Pair<FastConstructor, ExprEvaluator[]>(fastClass.GetConstructor(ctor), evaluators);
            }
            catch (EngineImportException ex)
            {
                throw new ExprValidationException("Failed to find a suitable constructor for type '" + targetClass.GetCleanName() + "': " + ex.Message, ex);
            }
        }
    }
} // end of namespace
