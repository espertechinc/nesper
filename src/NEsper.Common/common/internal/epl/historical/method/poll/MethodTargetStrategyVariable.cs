///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyVariable : MethodTargetStrategy
    {
        private readonly MethodTargetStrategyVariableFactory factory;
        private readonly VariableReader reader;

        public MethodTargetStrategyVariable(
            MethodTargetStrategyVariableFactory factory,
            VariableReader reader)
        {
            this.factory = factory;
            this.reader = reader;
        }

        public object Invoke(
            object lookupValues,
            AgentInstanceContext agentInstanceContext)
        {
            var target = reader.Value;
            if (target == null) {
                return null;
            }

            if (target is EventBean) {
                target = ((EventBean) target).Underlying;
            }

            try {
                switch (factory.invokeType) {
                    case MethodTargetStrategyStaticMethodInvokeType.NOPARAM:
                        return factory.method.Invoke(target, null);

                    case MethodTargetStrategyStaticMethodInvokeType.SINGLE:
                        return factory.method.Invoke(target, new object[] {lookupValues});

                    case MethodTargetStrategyStaticMethodInvokeType.MULTIKEY:
                        return factory.method.Invoke(target, (object[]) lookupValues);

                    default:
                        throw new IllegalStateException("Unrecognized value for " + factory.invokeType);
                }
            }
            catch (TargetException ex) {
                throw new EPException(
                    "Method '" +
                    factory.method.Name +
                    "' of class '" +
                    factory.method.DeclaringType.Name +
                    "' reported an exception: " +
                    ex.InnerException,
                    ex.InnerException);
            }
            catch (MemberAccessException ex) {
                throw new EPException(
                    "Method '" +
                    factory.method.Name +
                    "' of class '" +
                    factory.method.DeclaringType.Name +
                    "' reported an exception: " +
                    ex,
                    ex);
            }
        }

        public string Plan => "Variable '" + reader.MetaData.VariableName + "'";
    }
} // end of namespace