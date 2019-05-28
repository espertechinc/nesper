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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyStaticMethod : MethodTargetStrategy,
        MethodTargetStrategyFactory,
        StatementReadyCallback
    {
        private Type clazz;
        private MethodTargetStrategyStaticMethodInvokeType invokeType;
        private MethodInfo method;
        private string methodName;
        private Type[] methodParameters;

        public MethodTargetStrategyStaticMethodInvokeType InvokeType {
            set => invokeType = value;
        }

        public Type Clazz {
            set => clazz = value;
        }

        public string MethodName {
            set => methodName = value;
        }

        public Type[] MethodParameters {
            set => methodParameters = value;
        }

        public object Invoke(
            object lookupValues,
            AgentInstanceContext agentInstanceContext)
        {
            try {
                switch (invokeType) {
                    case MethodTargetStrategyStaticMethodInvokeType.NOPARAM:
                        return method.Invoke(null, null);
                    case MethodTargetStrategyStaticMethodInvokeType.SINGLE:
                        return method.Invoke(null, new[] {lookupValues});
                    case MethodTargetStrategyStaticMethodInvokeType.MULTIKEY:
                        return method.Invoke(null, ((HashableMultiKey) lookupValues).Keys);
                    default:
                        throw new IllegalStateException("Unrecognized value for " + invokeType);
                }
            }
            catch (TargetException ex) {
                throw new EPException(
                    "Method '" + method.Name + "' of class '" + method.DeclaringType.Name +
                    "' reported an exception: " + ex.InnerException, ex.InnerException);
            }
        }

        public string Plan => "method '" + methodName + "' of class '" + clazz.Name + "'";

        public MethodTargetStrategy Make(AgentInstanceContext agentInstanceContext)
        {
            return this;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            method = ResolveMethod(clazz, methodName, methodParameters);
            invokeType = MethodTargetStrategyStaticMethodInvokeTypeExtensions.GetInvokeType(method);
        }

        protected internal static MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] methodParameters)
        {
            MethodInfo method;
            try {
                method = clazz.GetMethod(methodName, methodParameters);
            }
            catch (Exception ex)
            when (ex is AmbiguousMatchException || ex is ArgumentNullException)
            {
                throw new EPException(
                    "Failed to find method '" + methodName + "' of class '" + clazz.Name + "' with parameters " +
                    TypeHelper.GetParameterAsString(methodParameters));
            }

            return method;
        }
    }
} // end of namespace