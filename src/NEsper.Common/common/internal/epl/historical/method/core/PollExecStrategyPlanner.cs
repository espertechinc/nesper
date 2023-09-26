///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.historical.method.poll;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class PollExecStrategyPlanner
    {
        public static Pair<MethodTargetStrategyForge, MethodConversionStrategyForge> Plan(
            MethodPollingViewableMeta metadata,
            MethodInfo targetMethod,
            EventType eventType)
        {
            MethodTargetStrategyForge target = null;
            MethodConversionStrategyForge conversion = null;

            // class-based evaluation
            if (metadata.MethodProviderClass != null) {
                // Construct polling strategy as a method invocation
                var strategy = metadata.Strategy;
                var variable = metadata.Variable;
                if (variable == null) {
                    target = new MethodTargetStrategyStaticMethodForge(metadata.MethodProviderClass, targetMethod);
                }
                else {
                    target = new MethodTargetStrategyVariableForge(variable, targetMethod);
                }

                if (metadata.EventTypeEventBeanArray != null) {
                    conversion = new MethodConversionStrategyForge(
                        eventType,
                        typeof(MethodConversionStrategyEventBeans));
                }
                else if (metadata.OptionalMapType != null) {
                    if (targetMethod.ReturnType.IsArray) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyArrayMap));
                    }
                    else if (metadata.IsCollection) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyCollectionMap));
                    }
                    else if (metadata.IsIterator) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyIteratorMap));
                    }
                    else {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyPlainMap));
                    }
                }
                else if (metadata.OptionalOaType != null) {
                    if (targetMethod.ReturnType == typeof(object[][])) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyArrayOA));
                    }
                    else if (metadata.IsCollection) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyCollectionOA));
                    }
                    else if (metadata.IsIterator) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyIteratorOA));
                    }
                    else {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyPlainOA));
                    }
                }
                else {
                    if (targetMethod.ReturnType.IsArray) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyArrayPONO));
                    }
                    else if (metadata.IsCollection) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyCollectionPONO));
                    }
                    else if (metadata.IsIterator) {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyIteratorPONO));
                    }
                    else {
                        conversion = new MethodConversionStrategyForge(
                            eventType,
                            typeof(MethodConversionStrategyPlainPONO));
                    }
                }
            }
            else {
                target = new MethodTargetStrategyScriptForge(metadata.ScriptExpression);
                conversion = new MethodConversionStrategyForge(eventType, typeof(MethodConversionStrategyScript));
            }

            return new Pair<MethodTargetStrategyForge, MethodConversionStrategyForge>(target, conversion);
        }
    }
} // end of namespace