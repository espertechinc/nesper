///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public class EventBeanSummarizer
    {
        public static string Summarize(EventBean theEvent)
        {
            if (theEvent == null) {
                return "(null)";
            }

            var writer = new StringWriter();
            Summarize(theEvent, writer);
            return writer.ToString();
        }

        public static void Summarize(
            EventBean theEvent,
            TextWriter writer)
        {
            if (theEvent == null) {
                writer.Write("(null)");
                return;
            }

            writer.Write(theEvent.EventType.Name);
            writer.Write("[");
            SummarizeUnderlying(theEvent.Underlying, writer);
            writer.Write("]");
        }

        public static string SummarizeUnderlying(object underlying)
        {
            if (underlying == null) {
                return "(null)";
            }

            var writer = new StringWriter();
            SummarizeUnderlying(underlying, writer);
            return writer.ToString();
        }

        public static void SummarizeUnderlying(
            object underlying,
            TextWriter writer)
        {
            underlying.RenderAny(writer);
        }

        public static string Summarize(EventBean[] events)
        {
            if (events == null) {
                return "(null)";
            }

            if (events.Length == 0) {
                return "(empty)";
            }

            var writer = new StringWriter();
            var delimiter = "";
            for (var i = 0; i < events.Length; i++) {
                writer.Write(delimiter);
                writer.Write("event ");
                writer.Write(Convert.ToString(i));
                writer.Write(":");
                Summarize(events[i], writer);
                delimiter = ", ";
            }

            return writer.ToString();
        }
    }
} // end of namespace