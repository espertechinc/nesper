///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Castle.DynamicProxy;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.view.stream
{
    public class EventStreamProxy : IInterceptor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly String _eventTypeAndFilter;
        private readonly EventStream _eventStream;

        public static Object NewInstance(String engineURI, String statementName, String eventTypeAndFilter, EventStream eventStream)
        {
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithoutTarget(
                typeof(EventStream),
                eventStream.GetType().GetInterfaces(),
                new EventStreamProxy(engineURI, statementName, eventTypeAndFilter, eventStream));
        }

        public static EventStream GetAuditProxy(String engineURI, String statementName, Attribute[] annotations, FilterSpecCompiled filterSpec, EventStream designated)
        {
            var audit = AuditEnum.STREAM.GetAudit(annotations);
            if (audit == null)
            {
                return designated;
            }

            var writer = new StringWriter();
            writer.Write(filterSpec.FilterForEventType.Name);
            if (filterSpec.Parameters != null && filterSpec.Parameters.Length > 0)
            {
                writer.Write('(');
                String delimiter = "";
                foreach (FilterSpecParam[] paramLine in filterSpec.Parameters)
                {
                    writer.Write(delimiter);
                    WriteFilter(writer, paramLine);
                    delimiter = " or ";
                }
                writer.Write(')');
            }

            return (EventStream) EventStreamProxy.NewInstance(engineURI, statementName, writer.ToString(), designated);
        }

        private static void WriteFilter(StringWriter writer, FilterSpecParam[] paramLine)
        {
            String delimiter = "";
            foreach (FilterSpecParam param in paramLine)
            {
                writer.Write(delimiter);
                writer.Write(param.Lookupable.Expression);
                writer.Write(param.FilterOperator.GetTextualOp());
                writer.Write("...");
                delimiter = ",";
            }
        }

        public EventStreamProxy(String engineURI, String statementName, String eventTypeAndFilter, EventStream eventStream)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _eventTypeAndFilter = eventTypeAndFilter;
            _eventStream = eventStream;
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name == "Insert")
            {
                if (AuditPath.IsInfoEnabled)
                {
                    var arg = invocation.Arguments[0];
                    var events = "(undefined)";
                    if (arg is EventBean[])
                    {
                        events = EventBeanUtility.Summarize((EventBean[])arg);
                    }
                    else if (arg is EventBean)
                    {
                        events = EventBeanUtility.Summarize((EventBean)arg);
                    }
                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.STREAM, _eventTypeAndFilter + " inserted " + events);

                }
            }

            invocation.ReturnValue = invocation.Method.Invoke(_eventStream, invocation.Arguments);
        }
    }
}
