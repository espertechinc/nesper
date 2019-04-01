///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Castle.DynamicProxy;

using com.espertech.esper.client.annotation;
using com.espertech.esper.util;

namespace com.espertech.esper.schedule
{
    public class ScheduleHandleCallbackProxy : IInterceptor
    {
        private static readonly MethodInfo ScheduledTriggerMethod = typeof(ScheduleHandleCallback).GetMethod("ScheduledTrigger");

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly ScheduleHandleCallback _scheduleHandleCallback;

        public static ScheduleHandleCallback NewInstance(String engineURI, String statementName, ScheduleHandleCallback scheduleHandleCallback)
        {
            var generator = new ProxyGenerator();
            var interfaces = scheduleHandleCallback
                .GetType()
                .GetInterfaces()
                .Where(ii => ii != typeof(IProxyTargetAccessor))
                .ToArray();

            return (ScheduleHandleCallback) generator.CreateInterfaceProxyWithoutTarget(
                typeof(ScheduleHandleCallback), interfaces, new ScheduleHandleCallbackProxy(engineURI, statementName, scheduleHandleCallback));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleHandleCallbackProxy"/> class.
        /// </summary>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="scheduleHandleCallback">The schedule handle callback.</param>
        public ScheduleHandleCallbackProxy(String engineURI, String statementName, ScheduleHandleCallback scheduleHandleCallback)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _scheduleHandleCallback = scheduleHandleCallback;
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == ScheduledTriggerMethod)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    var message = new StringWriter();
                    message.Write("trigger handle ");
                    TypeHelper.WriteInstance(message, _scheduleHandleCallback, true);
                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.SCHEDULE, message.ToString());
                }
            }

            invocation.ReturnValue =
                invocation.Method.Invoke(_scheduleHandleCallback, invocation.Arguments);
        }
    }
}