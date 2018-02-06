///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.client
{
    public class MySingleRowFunction
    {
        private static readonly IList<EPLMethodInvocationContext> _methodInvokeContexts =
            new List<EPLMethodInvocationContext>();

        public static IList<EPLMethodInvocationContext> MethodInvokeContexts
        {
            get { return _methodInvokeContexts; }
        }

        public static int ComputePower3(int i)
        {
            return i*i*i;
        }

        public static int ComputePower3WithContext(int i, EPLMethodInvocationContext context)
        {
            _methodInvokeContexts.Add(context);
            return i*i*i;
        }

        public static String Surroundx(String target)
        {
            return "X" + target + "X";
        }

        public static InnerSingleRow GetChainTop()
        {
            return new InnerSingleRow();
        }

        public static void ThrowException()
        {
            throw new Exception("This is a 'throwexception' generated exception");
        }

        public static bool IsNullValue(EventBean @event, String propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        public static String GetValueAsString(EventBean @event, String propertyName)
        {
            var result = @event.Get(propertyName);
            return result != null ? result.ToString() : null;
        }

        public class InnerSingleRow
        {
            public int ChainValue(int i, int j)
            {
                return i*j;
            }
        }

        public static bool EventsCheckStrings(ICollection<EventBean> events, String property, String value)
        {
            return events.Any(@event => @event.Get(property).Equals(value));
        }

        public static String VarargsOnlyInt(params int[] values)
        {
            var objects = new Object[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                objects[i] = values[i];
            }
            return ToCSV(objects);
        }

        public static String VarargsW1Param(String first, params double[] values)
        {
            var objects = new Object[values.Length + 1];
            objects[0] = first;
            for (var i = 0; i < values.Length; i++)
            {
                objects[i + 1] = values[i];
            }
            return ToCSV(objects);
        }

        public static String VarargsW2Param(int first, double second, params long?[] values)
        {
            var objects = new Object[values.Length + 2];
            objects[0] = first;
            objects[1] = second;
            for (var i = 0; i < values.Length; i++)
            {
                objects[i + 2] = values[i];
            }
            return ToCSV(objects);
        }

        public static String VarargsOnlyWCtx(EPLMethodInvocationContext ctx, params int[] values)
        {
            return "CTX+" + VarargsOnlyInt(values);
        }

        public static String VarargsW1ParamWCtx(String first, EPLMethodInvocationContext ctx, params int?[] values)
        {
            return "CTX+" + first + "," + ToCSV(values);
        }

        public static String VarargsW2ParamWCtx(
            String first,
            String second,
            EPLMethodInvocationContext ctx,
            params int?[] values)
        {
            return "CTX+" + first + "," + second + "," + ToCSV(values);
        }

        public static String VarargsOnlyObject(params object[] values)
        {
            return ToCSV(values);
        }

        public static String VarargsOnlyString(params string[] values)
        {
            return ToCSV(values);
        }

        public static String VarargsObjectsWCtx(EPLMethodInvocationContext ctx, params object[] values)
        {
            return "CTX+" + ToCSV(values);
        }

        public static String VarargsW1ParamObjectsWCtx(
            int param,
            EPLMethodInvocationContext ctx,
            params object[] values)
        {
            return "CTX+" + "," + param + "," + ToCSV(values);
        }

        public static String VarargsOnlyNumber(params object[] values)
        {
            return ToCSV(values);
        }

        public static String VarargsOnlyISupportBaseAB(params ISupportBaseAB[] values)
        {
            return ToCSV(values);
        }

        private static String ToCSV<T>(IEnumerable<T> values)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in values)
            {
                writer.Write(delimiter);
                writer.Write(CompatExtensions.RenderAny(item));
                delimiter = ",";
            }
            return writer.ToString();
        }
    }
}
