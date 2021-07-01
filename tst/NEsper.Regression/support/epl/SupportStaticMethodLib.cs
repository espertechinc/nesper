///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.support.epl
{
    public class SupportStaticMethodLib
    {
        public static IList<object[]> Invocations { get; } = new List<object[]>();

        public static IList<EPLMethodInvocationContext> MethodInvocationContexts { get; } =
            new List<EPLMethodInvocationContext>();

        public static int CountInvoked { get; private set; }

        public static EventBean[] EventBeanArrayForString(
            string value,
            EPLMethodInvocationContext context)
        {
            var split = value.SplitCsv();
            var events = new EventBean[split.Length];
            for (var i = 0; i < split.Length; i++) {
                events[i] = context.EventBeanService.AdapterForMap(
                    Collections.SingletonDataMap("p0", split[i]),
                    "MyItemEvent");
            }

            return events;
        }

        public static ICollection<EventBean> EventBeanCollectionForString(
            string value,
            EPLMethodInvocationContext context)
        {
            return Arrays.AsList(EventBeanArrayForString(value, context));
        }

        public static IEnumerator<EventBean> EventBeanIteratorForString(
            string value,
            EPLMethodInvocationContext context)
        {
            return EventBeanCollectionForString(value, context).GetEnumerator();
        }

        public static bool CompareEvents(
            SupportMarketDataBean beanOne,
            SupportBean beanTwo)
        {
            return beanOne.Symbol.Equals(beanTwo.TheString);
        }

        public static IDictionary<string, object> FetchMapArrayMRMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static LinkedHashMap<string, object> FetchObjectArrayEventBeanMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static LinkedHashMap<string, object> FetchOAArrayMRMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static IDictionary<string, object> FetchSingleValueMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("result", typeof(int?));
            return values;
        }

        public static IDictionary<string, object>[] FetchResult12(int? value)
        {
            if (value == null) {
                return new IDictionary<string, object>[0];
            }

            var result = new IDictionary<string, object>[2];
            result[0] = new Dictionary<string, object>();
            result[0].Put("value", 1);
            result[1] = new Dictionary<string, object>();
            result[1].Put("value", 2);
            return result;
        }

        public static IDictionary<string, object> FetchResult12Metadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("value", typeof(int?));
            return values;
        }

        public static IDictionary<string, object>[] FetchResult23(int? value)
        {
            if (value == null) {
                return new IDictionary<string, object>[0];
            }

            var result = new IDictionary<string, object>[2];
            result[0] = new Dictionary<string, object>();
            result[0].Put("value", 2);
            result[1] = new Dictionary<string, object>();
            result[1].Put("value", 3);
            return result;
        }

        public static IDictionary<string, object> FetchResult23Metadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("value", typeof(int?));
            values.Put("valueTwo", typeof(int?));
            return values;
        }

        public static string Join(SupportBean bean)
        {
            return bean.TheString + " " + Convert.ToString(bean.IntPrimitive);
        }

        public static IDictionary<string, object>[] FetchResult100()
        {
            var result = new IDictionary<string, object>[100];
            var count = 0;
            for (var i = 0; i < 10; i++) {
                for (var j = 0; j < 10; j++) {
                    result[count] = new Dictionary<string, object>();
                    result[count].Put("col1", i);
                    result[count].Put("col2", j);
                    count++;
                }
            }

            return result;
        }

        public static IDictionary<string, object> FetchResult100Metadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("col1", typeof(int?));
            values.Put("col2", typeof(int?));
            return values;
        }

        public static IDictionary<string, object>[] FetchBetween(
            int? lower,
            int? upper)
        {
            if (lower == null || upper == null) {
                return new IDictionary<string, object>[0];
            }

            if (upper < lower) {
                return new IDictionary<string, object>[0];
            }

            var delta = upper.Value - lower.Value + 1;
            var result = new IDictionary<string, object>[delta];
            var count = 0;
            for (var i = lower.Value; i <= upper; i++) {
                IDictionary<string, object> values = new Dictionary<string, object>();
                values.Put("value", i);
                result[count++] = values;
            }

            return result;
        }

        public static IDictionary<string, object>[] FetchBetweenString(
            int? lower,
            int? upper)
        {
            if (lower == null || upper == null) {
                return new IDictionary<string, object>[0];
            }

            if (upper < lower) {
                return new IDictionary<string, object>[0];
            }

            var delta = upper.Value - lower.Value + 1;
            var result = new IDictionary<string, object>[delta];
            var count = 0;
            for (var i = lower.Value; i <= upper; i++) {
                IDictionary<string, object> values = new Dictionary<string, object>();
                values.Put("value", Convert.ToString(i));
                result[count++] = values;
            }

            return result;
        }

        public static IDictionary<string, object> FetchBetweenMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("value", typeof(int?));
            return values;
        }

        public static IDictionary<string, object> FetchBetweenStringMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("value", typeof(string));
            return values;
        }

        public static IDictionary<string, object>[] FetchMapArrayMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new IDictionary<string, object>[0];
            }

            var rows = new IDictionary<string, object>[id];
            for (var i = 0; i < id; i++) {
                IDictionary<string, object> values = new Dictionary<string, object>();
                rows[i] = values;

                values.Put("mapstring", "|" + theString + "_" + i + "|");
                values.Put("mapint", i + 100);
            }

            return rows;
        }

        public static object[][] FetchOAArrayMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new object[0][];
            }

            var rows = new object[id][];
            for (var i = 0; i < id; i++) {
                var values = new object[2];
                rows[i] = values;

                values[0] = "|" + theString + "_" + i + "|";
                values[1] = i + 100;
            }

            return rows;
        }

        public static IDictionary<string, object> FetchMapMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static IDictionary<string, object> FetchMap(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            IDictionary<string, object> values = new Dictionary<string, object>();
            if (id == 0) {
                return values;
            }

            values.Put("mapstring", "|" + theString + "|");
            values.Put("mapint", id + 1);
            return values;
        }

        public static IDictionary<string, object> FetchMapEventBeanMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static IDictionary<string, object> FetchMapEventBean(
            EventBean eventBean,
            string propOne,
            string propTwo)
        {
            var theString = (string) eventBean.Get(propOne);
            var id = eventBean.Get(propTwo).AsInt32();
            if (id < 0) {
                return null;
            }

            IDictionary<string, object> values = new Dictionary<string, object>();
            if (id == 0) {
                return values;
            }

            values.Put("mapstring", "|" + theString + "|");
            values.Put("mapint", id + 1);
            return values;
        }

        public static IDictionary<string, object> FetchIdDelimitedMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("result", typeof(string));
            return values;
        }

        public static IDictionary<string, object> FetchIdDelimited(int? value)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("result", "|" + value + "|");
            return values;
        }

        public static IDictionary<string, object> ConvertEventMap(IDictionary<string, object> values)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put("one", values.Get("one"));
            result.Put("two", "|" + values.Get("two") + "|");
            return result;
        }

        public static object[] ConvertEventObjectArray(object[] values)
        {
            return new[] {values[0], "|" + values[1] + "|"};
        }

        public static GenericRecord ConvertEventAvro(GenericRecord row)
        {
            var val1 = row.Get("one").ToString();
            var val2 = row.Get("two").ToString();
            var upd = new GenericRecord(row.Schema);
            upd.Put("one", val1);
            upd.Put("two", "|" + val2 + "|");
            return upd;
        }

        public static String ConvertEventJson(JsonEventObject row)
        {
            var val1 = row.Get("one").ToString();
            var val2 = row.Get("two").ToString();
            var json = new JObject();
            json.Add("one", val1);
            json.Add("two", "|" + val2 + "|");
            return json.ToString();
        }

        public static SupportBean ConvertEvent(SupportMarketDataBean bean)
        {
            return new SupportBean(bean.Symbol, bean.Volume.AsInt32());
        }

        public static object StaticMethod(object @object)
        {
            return @object;
        }

        public static object StaticMethodWithContext(
            object @object,
            EPLMethodInvocationContext context)
        {
            MethodInvocationContexts.Add(context);
            return @object;
        }

        public static int ArrayLength(object @object)
        {
            if (@object is Array array) {
                return array.Length;
            }

            return -1;
        }

        public static void ThrowException()
        {
            throw new Exception("throwException text here");
        }

        public static SupportBean ThrowExceptionBeanReturn()
        {
            throw new Exception("throwException text here");
        }

        public static bool IsStringEquals(
            string value,
            string compareTo)
        {
            return value.Equals(compareTo);
        }

        public static double MinusOne(double value)
        {
            return value - 1;
        }

        public static int PlusOne(int value)
        {
            return value + 1;
        }

        public static string AppendPipe(
            string theString,
            string value)
        {
            return theString + "|" + value;
        }

        public static SupportBean_S0 FetchObjectAndSleep(
            string fetchId,
            int passThroughNumber,
            long msecSleepTime)
        {
            Thread.Sleep((int) msecSleepTime);
            return new SupportBean_S0(passThroughNumber, "|" + fetchId + "|");
        }

        public static FetchedData FetchObjectNoArg()
        {
            return new FetchedData("2");
        }

        public static FetchedData FetchObject(string id)
        {
            if (id == null) {
                return null;
            }

            return new FetchedData("|" + id + "|");
        }

        public static FetchedData[] FetchArrayNoArg()
        {
            return new[] {new FetchedData("1")};
        }

        public static FetchedData[] FetchArrayGen(int numGenerate)
        {
            if (numGenerate < 0) {
                return null;
            }

            if (numGenerate == 0) {
                return new FetchedData[0];
            }

            if (numGenerate == 1) {
                return new[] {new FetchedData("A")};
            }

            var fetched = new FetchedData[numGenerate];
            for (var i = 0; i < numGenerate; i++) {
                var c = 'A' + i;
                fetched[i] = new FetchedData(char.ToString((char) c));
            }

            return fetched;
        }

        public static long Passthru(long value)
        {
            return value;
        }

        public static void Sleep(long msec)
        {
            try {
                Thread.Sleep((int) msec);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException("Interrupted during sleep", e);
            }
        }

        public static bool SleepReturnTrue(long msec)
        {
            try {
                Thread.Sleep((int) msec);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException("Interrupted during sleep", e);
            }

            return true;
        }

        public static string DelimitPipe(string theString)
        {
            if (theString == null) {
                return "|<null>|";
            }

            return "|" + theString + "|";
        }

        public static bool VolumeGreaterZero(SupportMarketDataBean bean)
        {
            return bean.Volume > 0;
        }

        public static bool VolumeGreaterZeroEventBean(EventBean bean)
        {
            var volume = bean.Get("Volume").AsInt64();
            return volume > 0;
        }

        public static BigInteger MyBigIntFunc(BigInteger val)
        {
            return val;
        }

        public static decimal MyDecimalFunc(decimal val)
        {
            return val;
        }

        public static IDictionary<string, string> MyMapFunc()
        {
            IDictionary<string, string> map = new Dictionary<string, string>();
            map.Put("A", "A1");
            map.Put("B", "B1");
            return map;
        }

        public static int[] MyArrayFunc()
        {
            return new[] {100, 200, 300};
        }

        public static int ArraySumIntBoxed(int?[] array)
        {
            var sum = 0;
            for (var i = 0; i < array.Length; i++) {
                if (array[i] == null) {
                    continue;
                }

                sum += array[i].Value;
            }

            return sum;
        }

        public static double ArraySumDouble(double?[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++) {
                if (array[i] == null) {
                    continue;
                }

                sum += array[i].Value;
            }

            return sum;
        }

        public static double ArraySumString(string[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++) {
                if (array[i] == null) {
                    continue;
                }

                sum += double.Parse(array[i]);
            }

            return sum;
        }

        public static bool AlwaysTrue(object[] input)
        {
            Invocations.Add(input);
            return true;
        }

        public static bool AlwaysTrue()
        {
            Invocations.Add(null);
            return true;
        }

        public static double ArraySumObject(object[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++) {
                if (array[i] == null) {
                    continue;
                }

                if (array[i].IsNumber()) {
                    sum += array[i].AsDouble();
                }
                else {
                    sum += double.Parse(array[i].ToString());
                }
            }

            return sum;
        }

        public static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive)
        {
            return new SupportBean(theString, intPrimitive);
        }

        public static SupportBeanNumeric MakeSupportBeanNumeric(
            int? intOne,
            int? intTwo)
        {
            return new SupportBeanNumeric(intOne, intTwo);
        }

        public static object[] FetchObjectArrayEventBean(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            IDictionary<string, object> values = new Dictionary<string, object>();
            if (id == 0) {
                return new object[2];
            }

            var fields = new object[2];
            fields[0] = "|" + theString + "|";
            fields[1] = id + 1;
            return fields;
        }

        public static LinkedHashMap<string, object> FetchTwoRows3ColsMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("pKey0", typeof(string));
            values.Put("pkey1", typeof(int?));
            values.Put("c0", typeof(long?));
            return values;
        }

        public static IDictionary<string, object>[] FetchTwoRows3Cols()
        {
            IDictionary<string, object>[] result = {new Dictionary<string, object>(), new Dictionary<string, object>()};

            result[0].Put("pKey0", "E1");
            result[0].Put("pkey1", 10);
            result[0].Put("c0", 100L);

            result[1].Put("pKey0", "E2");
            result[1].Put("pkey1", 20);
            result[1].Put("c0", 200L);

            return result;
        }

        public static MyMethodReturn[] FetchPONOArray(
            string mystring,
            int myint)
        {
            if (myint < 0) {
                return null;
            }

            if (myint == 0) {
                return new[] {new MyMethodReturn(null, null)};
            }

            return new[] {new MyMethodReturn("|" + mystring + "|", myint + 1)};
        }

        public static ICollection<MyMethodReturn> FetchPONOCollection(
            string mystring,
            int myint)
        {
            if (myint < 0) {
                return null;
            }

            if (myint == 0) {
                return Collections.SingletonList(new MyMethodReturn(null, null));
            }

            return Collections.SingletonList(new MyMethodReturn("|" + mystring + "|", myint + 1));
        }

        public static IEnumerator<MyMethodReturn> FetchPONOIterator(
            string mystring,
            int myint)
        {
            if (myint < 0) {
                return null;
            }

            return FetchPONOCollection(mystring, myint).GetEnumerator();
        }

        public static MyMethodReturn[] FetchPONOArrayMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new MyMethodReturn[0];
            }

            var rows = new MyMethodReturn[id];
            for (var i = 0; i < id; i++) {
                rows[i] = new MyMethodReturn("|" + theString + "_" + i + "|", i + 100);
            }

            return rows;
        }

        public static ICollection<MyMethodReturn> FetchPONOCollectionMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new EmptyList<MyMethodReturn>();
            }

            IList<MyMethodReturn> rows = new List<MyMethodReturn>(id);
            for (var i = 0; i < id; i++) {
                rows.Add(new MyMethodReturn("|" + theString + "_" + i + "|", i + 100));
            }

            return rows;
        }

        public static IEnumerator<MyMethodReturn> FetchPONOIteratorMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            return FetchPONOCollectionMR(theString, id).GetEnumerator();
        }

        public static IDictionary<string, object> FetchMapCollectionMRMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static ICollection<IDictionary<string, object>> FetchMapCollectionMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new EmptyList<IDictionary<string, object>>();
            }

            IList<IDictionary<string, object>> rows = new List<IDictionary<string, object>>(id);
            for (var i = 0; i < id; i++) {
                IDictionary<string, object> values = new Dictionary<string, object>();
                rows.Add(values);

                values.Put("mapstring", "|" + theString + "_" + i + "|");
                values.Put("mapint", i + 100);
            }

            return rows;
        }

        public static IDictionary<string, object> FetchMapIteratorMRMetadata()
        {
            return FetchMapCollectionMRMetadata();
        }

        public static IEnumerator<IDictionary<string, object>> FetchMapIteratorMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            return FetchMapCollectionMR(theString, id).GetEnumerator();
        }

        public static LinkedHashMap<string, object> FetchOACollectionMRMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int?));
            return values;
        }

        public static ICollection<object[]> FetchOACollectionMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            if (id == 0) {
                return new EmptyList<object[]>();
            }

            IList<object[]> rows = new List<object[]>(id);
            for (var i = 0; i < id; i++) {
                var values = new object[2];
                rows.Add(values);

                values[0] = "|" + theString + "_" + i + "|";
                values[1] = i + 100;
            }

            return rows;
        }

        public static IDictionary<string, object> FetchOAIteratorMRMetadata()
        {
            return FetchOACollectionMRMetadata();
        }

        public static IEnumerator<object[]> FetchOAIteratorMR(
            string theString,
            int id)
        {
            if (id < 0) {
                return null;
            }

            return FetchOACollectionMR(theString, id).GetEnumerator();
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin()
        {
            return GetOverloadedMethodForJoinResult("A", "B");
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin(int first)
        {
            return GetOverloadedMethodForJoinResult(Convert.ToString(first), "B");
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin(string first)
        {
            return GetOverloadedMethodForJoinResult(first, "B");
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin(
            string first,
            int second)
        {
            return GetOverloadedMethodForJoinResult(first, Convert.ToString(second));
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin(
            int first,
            int second)
        {
            return GetOverloadedMethodForJoinResult(Convert.ToString(first), Convert.ToString(second));
        }

        public static IDictionary<string, object> OverloadedMethodForJoinMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("col1", typeof(string));
            values.Put("col2", typeof(string));
            return values;
        }

        public static SupportBean_S0[] InvalidOverloadForJoin(string first)
        {
            return null;
        }

        public static SupportBean_S1[] InvalidOverloadForJoin(int? first)
        {
            return null;
        }

        private static IDictionary<string, object>[] GetOverloadedMethodForJoinResult(
            string first,
            string second)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("col1", first);
            values.Put("col2", second);
            return new[] {values};
        }

        public static int LibSplit(string theString)
        {
            var key = theString.Split('_');
            CountInvoked++;
            return int.Parse(key[1]);
        }

        public static bool LibE1True(string theString)
        {
            CountInvoked++;
            return theString.Equals("E_1");
        }

        public static void ResetCountInvoked()
        {
            CountInvoked = 0;
        }

        [Serializable]
        public class FetchedData
        {
            private string id;

            public FetchedData(string id)
            {
                this.id = id;
            }

            public string Id {
                get => id;
                set => id = value;
            }
        }

        [Serializable]
        public class MyMethodReturn
        {
            public MyMethodReturn(
                string mapstring,
                int? mapint)
            {
                Mapstring = mapstring;
                Mapint = mapint;
            }

            [PropertyName("mapstring")]
            public string Mapstring { get; }

            [PropertyName("mapint")]
            public int? Mapint { get; }
        }
    }
} // end of namespace