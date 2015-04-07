///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using Castle.DynamicProxy;
using com.espertech.esper.client.annotation;
using com.espertech.esper.util;

namespace com.espertech.esper.schedule
{
    public class ScheduleHandleCallbackProxy : IInterceptor
    {

        private static readonly MethodInfo Target = typeof(ScheduleHandleCallback).GetMethod("ScheduledTrigger");

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly ScheduleHandleCallback _scheduleHandleCallback;

        public static Object NewInstance(String engineURI, String statementName, ScheduleHandleCallback scheduleHandleCallback)
        {
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithoutTarget(
                typeof(ScheduleHandleCallback),
                scheduleHandleCallback.GetType().GetInterfaces(),
                new ScheduleHandleCallbackProxy(engineURI, statementName, scheduleHandleCallback));
        }

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
            if (invocation.Method == Target)
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