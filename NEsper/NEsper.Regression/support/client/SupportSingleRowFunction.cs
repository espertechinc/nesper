///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportSingleRowFunction
    {
        public static IList<EPLMethodInvocationContext> MethodInvocationContexts { get; } =
            new List<EPLMethodInvocationContext>();

        public static int ComputePower3(int i)
        {
            return i * i * i;
        }

        public static int ComputePower3WithContext(
            int i,
            EPLMethodInvocationContext context)
        {
            MethodInvocationContexts.Add(context);
            return i * i * i;
        }

        public static string Surroundx(string target)
        {
            return "X" + target + "X";
        }

        public static InnerSingleRow GetChainTop()
        {
            return new InnerSingleRow();
        }

        public static void Throwexception()
        {
            throw new EPException("This is a 'throwexception' generated exception");
        }

        public static bool IsNullValue(
            EventBean @event,
            string propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        public static string GetValueAsString(
            EventBean @event,
            string propertyName)
        {
            var result = @event.Get(propertyName);
            return result != null ? result.ToString() : null;
        }

        public static bool EventsCheckStrings(
            ICollection<EventBean> events,
            string property,
            string value)
        {
            foreach (var @event in events) {
                if (@event.Get(property).Equals(value)) {
                    return true;
                }
            }

            return false;
        }

        public static string VarargsOnlyInt(params int[] values)
        {
            var objects = new object[values.Length];
            for (var i = 0; i < values.Length; i++) {
                objects[i] = values[i];
            }

            return ToCSV(objects);
        }

        public static string VarargsW1Param(
            string first,
            params double[] values)
        {
            var objects = new object[values.Length + 1];
            objects[0] = first;
            for (var i = 0; i < values.Length; i++) {
                objects[i + 1] = values[i];
            }

            return ToCSV(objects);
        }

        public static string VarargsW2Param(
            int first,
            double second,
            params long[] values)
        {
            var objects = new object[values.Length + 2];
            objects[0] = first;
            objects[1] = second;
            for (var i = 0; i < values.Length; i++) {
                objects[i + 2] = values[i];
            }

            return ToCSV(objects);
        }

        public static string VarargsOnlyWCtx(
            EPLMethodInvocationContext ctx,
            params int[] values)
        {
            return "CTX+" + VarargsOnlyInt(values);
        }

        public static string VarargsW1ParamWCtx(
            string first,
            EPLMethodInvocationContext ctx,
            params int[] values)
        {
            return "CTX+" + first + "," + ToCSV(values);
        }

        public static string VarargsW2ParamWCtx(
            string first,
            string second,
            EPLMethodInvocationContext ctx,
            params int[] values)
        {
            return "CTX+" + first + "," + second + "," + ToCSV(values);
        }

        public static string VarargsOnlyObject(params object[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsOnlyString(params string[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsObjectsWCtx(
            EPLMethodInvocationContext ctx,
            params object[] values)
        {
            return "CTX+" + ToCSV(values);
        }

        public static string VarargsOnlyBoxedFloat(params float?[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsOnlyBoxedShort(params short?[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsOnlyBoxedByte(params byte?[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsOnlyBigInt(params BigInteger[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsW1ParamObjectsWCtx(
            int param,
            EPLMethodInvocationContext ctx,
            params object[] values)
        {
            return "CTX+" + "," + param + "," + ToCSV(values);
        }

        public static string VarargsOnlyNumber(params object[] values)
        {
            return ToCSV(values);
        }

        public static string VarargsOnlyISupportBaseAB(params ISupportBaseAB[] values)
        {
            return ToCSV(values);
        }

        public static string VarargOverload(int a)
        {
            return "P1";
        }

        public static string VarargOverload(
            int a,
            int b)
        {
            return "P2";
        }

        public static string VarargOverload(
            int a,
            int b,
            int c)
        {
            return "p3";
        }

        public static string VarargOverload(params int[] many)
        {
            return "many";
        }

        private static string ToCSV<T>(T[] values)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (object item in values) {
                writer.Write(delimiter);
                writer.Write(item.RenderAny());
                delimiter = ",";
            }

            return writer.ToString();
        }

        public class InnerSingleRow
        {
            public int ChainValue(
                int i,
                int j)
            {
                return i * j;
            }
        }
    }
} // end of namespace