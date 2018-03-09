///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;

using Avro.Generic;

using NEsper.Avro.Extensions;

namespace com.espertech.esper.supportregression.epl
{
    using DataMap = IDictionary<string, object>;
    using TypeMap = IDictionary<string, object>;

    public class SupportStaticMethodLib
    {
        private static readonly IList<object[]> invocations = new List<object[]>();
        private static readonly IList<EPLMethodInvocationContext> methodInvocationContexts = new List<EPLMethodInvocationContext>();

        public static IList<object[]> Invocations
        {
            get { return invocations; }
        }

        public static IList<EPLMethodInvocationContext> GetMethodInvocationContexts()
        {
            return methodInvocationContexts;
        }

        public static EventBean[] EventBeanArrayForString(String value, EPLMethodInvocationContext context)
        {
            String[] split = value.Split(',');
            EventBean[] events = new EventBean[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                events[i] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("p0", split[i]), "MyItemEvent");
            }
            return events;
        }

        public static ICollection<EventBean> EventBeanCollectionForString(String value, EPLMethodInvocationContext context)
        {
            return new List<EventBean>(EventBeanArrayForString(value, context));
        }

        public static IEnumerator<EventBean> EventBeanIteratorForString(String value, EPLMethodInvocationContext context)
        {
            return EventBeanCollectionForString(value, context).GetEnumerator();
        }

        public static bool CompareEvents(SupportMarketDataBean beanOne, SupportBean beanTwo)
        {
            return beanOne.Symbol.Equals(beanTwo.TheString);
        }

        public static TypeMap FetchMapArrayMRMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(String));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static IDictionary<string, object> FetchObjectArrayEventBeanMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static IDictionary<string, object> FetchOAArrayMRMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static TypeMap FetchSingleValueMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("result", typeof(int));
            return values;
        }

        public static IDictionary<string, object>[] FetchResult12(int? value)
        {
            if (value == null)
            {
                return new IDictionary<string, object>[0];
            }

            var result = new IDictionary<string, object>[2];
            result[0] = new Dictionary<string, object>();
            result[0].Put("value", 1);
            result[1] = new Dictionary<string, object>();
            result[1].Put("value", 2);
            return result;
        }

        public static TypeMap FetchResult12Metadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("value", typeof(int));
            return values;
        }

        public static IDictionary<string, object>[] FetchResult23(int? value)
        {
            if (value == null)
            {
                return new IDictionary<string, object>[0];
            }

            var result = new IDictionary<string, object>[2];
            result[0] = new Dictionary<string, object>();
            result[0].Put("value", 2);
            result[1] = new Dictionary<string, object>();
            result[1].Put("value", 3);
            return result;
        }

        public static TypeMap FetchResult23Metadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("value", typeof(int));
            values.Put("valueTwo", typeof(int));
            return values;
        }

        public static String Join(SupportBean bean)
        {
            return bean.TheString + " " + Convert.ToString(bean.IntPrimitive);
        }

        public static IDictionary<string, object>[] FetchResult100()
        {
            var result = new IDictionary<string, object>[100];
            var count = 0;
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    result[count] = new Dictionary<string, object>();
                    result[count].Put("col1", i);
                    result[count].Put("col2", j);
                    count++;
                }
            }
            return result;
        }

        public static TypeMap FetchResult100Metadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("col1", typeof(int));
            values.Put("col2", typeof(int));
            return values;
        }

        public static IDictionary<string, object>[] FetchBetween(int? lower, int? upper)
        {
            if (lower == null || upper == null)
            {
                return new IDictionary<string, object>[0];
            }

            if (upper < lower)
            {
                return new IDictionary<string, object>[0];
            }

            var delta = upper.Value - lower.Value + 1;
            var result = new IDictionary<string, object>[delta];
            var count = 0;
            for (var i = lower.Value; i <= upper; i++)
            {
                var values = new Dictionary<string, object>();
                values.Put("value", i);
                result[count++] = values;
            }
            return result;
        }

        public static IDictionary<string, string>[] FetchBetweenString(int? lower, int? upper)
        {
            if (lower == null || upper == null)
            {
                return new IDictionary<string, string>[0];
            }

            if (upper < lower)
            {
                return new IDictionary<string, string>[0];
            }

            var delta = upper.Value - lower.Value + 1;
            var result = new IDictionary<string, string>[delta];
            var count = 0;
            for (var i = lower.Value; i <= upper; i++)
            {
                var values = new Dictionary<string, string>();
                values.Put("value", Convert.ToString(i));
                result[count++] = values;
            }
            return result;
        }

        public static TypeMap FetchBetweenMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("value", typeof(int));
            return values;
        }

        public static TypeMap FetchBetweenStringMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("value", typeof(string));
            return values;
        }

        public static DataMap[] FetchMapArrayMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return new DataMap[0];
            }

            var rows = new DataMap[id];
            for (var i = 0; i < id; i++)
            {
                var values = new Dictionary<string, object>();
                rows[i] = values;

                values.Put("mapstring", $"|{theString}_{i}|");
                values.Put("mapint", i + 100);
            }

            return rows;
        }

        public static object[][] FetchOAArrayMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return new Object[0][];
            }

            var rows = new Object[id][];
            for (var i = 0; i < id; i++)
            {
                var values = new Object[2];
                rows[i] = values;

                values[0] = $"|{theString}_{i}|";
                values[1] = i + 100;
            }

            return rows;
        }

        public static TypeMap FetchMapMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(String));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static DataMap FetchMap(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            var values = new Dictionary<string, object>();
            if (id == 0)
            {
                return values;
            }

            values.Put("mapstring", $"|{theString}|");
            values.Put("mapint", id + 1);
            return values;
        }

        public static TypeMap FetchMapEventBeanMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(String));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static IDictionary<String, Object> FetchMapEventBean(EventBean eventBean, String propOne, String propTwo)
        {
            var theString = (String)eventBean.Get(propOne);
            var id = eventBean.Get(propTwo).AsInt();

            if (id < 0)
            {
                return null;
            }

            var values = new Dictionary<String, Object>();
            if (id == 0)
            {
                return values;
            }

            values.Put("mapstring", $"|{theString}|");
            values.Put("mapint", id + 1);
            return values;
        }

        public static TypeMap FetchIdDelimitedMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("result", typeof(String));
            return values;
        }

        public static IDictionary<String, Object> FetchIdDelimited(int? value)
        {
            var values = new Dictionary<string, object>();
            values.Put("result", "|" + value + "|");
            return values;
        }

        public static IDictionary<String, Object> ConvertEventMap(IDictionary<String, Object> values)
        {
            var result = new Dictionary<string, object>();
            result.Put("one", values.Get("one"));
            result.Put("two", "|" + values.Get("two") + "|");
            return result;
        }

        public static object[] ConvertEventObjectArray(object[] values)
        {
            return new object[] { values[0], "|" + values[1] + "|" };
        }

        public static GenericRecord ConvertEventAvro(GenericRecord row)
        {
            String val1 = row.Get("one").ToString();
            String val2 = row.Get("two").ToString();
            GenericRecord upd = new GenericRecord(row.Schema);
            upd.Put("one", val1);
            upd.Put("two", "|" + val2 + "|");
            return upd;
        }

        public static SupportBean ConvertEvent(SupportMarketDataBean bean)
        {
            return new SupportBean(bean.Symbol, (bean.Volume).AsInt());
        }

        public static Object StaticMethod(Object @object)
        {
            return @object;
        }

        public static Object StaticMethodWithContext(Object @object, EPLMethodInvocationContext context)
        {
            methodInvocationContexts.Add(context);
            return @object;
        }

        public static int ArrayLength(Object @object)
        {
            var asArray = @object as Array;
            if (asArray == null)
            {
                return -1;
            }

            return asArray.Length;
        }

        public static void ThrowException()
        {
            throw new Exception("throwException text here");
        }

        public static SupportBean ThrowExceptionBeanReturn()
        {
            throw new Exception("throwException text here");
        }

        public static bool IsStringEquals(String value, String compareTo)
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

        public static String AppendPipe(String theString, String value)
        {
            return theString + "|" + value;
        }

        public static SupportBean_S0 FetchObjectAndSleep(String fetchId, int passThroughNumber, long msecSleepTime)
        {
            try
            {
                Thread.Sleep((int)msecSleepTime);
            }
            catch (ThreadInterruptedException)
            {
            }
            return new SupportBean_S0(passThroughNumber, "|" + fetchId + "|");
        }

        public static FetchedData FetchObjectNoArg()
        {
            return new FetchedData("2");
        }

        public static FetchedData FetchObject(String id)
        {
            if (id == null)
            {
                return null;
            }
            return new FetchedData("|" + id + "|");
        }

        public static FetchedData[] FetchArrayNoArg()
        {
            return new FetchedData[] { new FetchedData("1") };
        }

        public static FetchedData[] FetchArrayGen(int numGenerate)
        {
            if (numGenerate < 0)
            {
                return null;
            }
            if (numGenerate == 0)
            {
                return new FetchedData[0];
            }
            if (numGenerate == 1)
            {
                return new FetchedData[] { new FetchedData("A") };
            }

            var fetched = new FetchedData[numGenerate];
            for (var i = 0; i < numGenerate; i++)
            {
                var c = 'A' + i;
                fetched[i] = new FetchedData(((char)c).ToString(CultureInfo.InvariantCulture));
            }
            return fetched;
        }

        public static long Passthru(long value)
        {
            return value;
        }

        public static void Sleep(long msec)
        {
            try
            {
                Thread.Sleep((int)msec);
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception("Interrupted during sleep", e);
            }
        }

        public static bool SleepReturnTrue(long msec)
        {
            try
            {
                Thread.Sleep((int)msec);
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception("Interrupted during sleep", e);
            }
            return true;
        }

        public static String DelimitPipe(String theString)
        {
            if (theString == null)
            {
                return "|<null>|";
            }
            return $"|{theString}|";
        }

        public class FetchedData
        {
            public FetchedData(String id)
            {
                Id = id;
            }

            public string Id { get; set; }
        }

        public static bool VolumeGreaterZero(SupportMarketDataBean bean)
        {
            return bean.Volume > 0;
        }

        public static bool VolumeGreaterZeroEventBean(EventBean bean)
        {
            var volume = bean.Get("volume").AsLong();
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

        public static DataMap MyMapFunc()
        {
            var map = new Dictionary<string, object>();
            map.Put("A", "A1");
            map.Put("B", "B1");
            return map;
        }

        public static int[] MyArrayFunc()
        {
            return new int[] { 100, 200, 300 };
        }

        public static int ArraySumIntBoxed(int?[] array)
        {
            var sum = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                sum += array[i].Value;
            }
            return sum;
        }

        public static double ArraySumDouble(double?[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                sum += array[i].Value;
            }
            return sum;
        }

        public static double ArraySumString(String[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                sum += double.Parse(array[i]);
            }
            return sum;
        }

        public static bool AlwaysTrue(object[] input)
        {
            invocations.Add(input);
            return true;
        }

        public static double ArraySumObject(object[] array)
        {
            double sum = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                sum += array[i].AsDouble();
            }
            return sum;
        }

        public static SupportBean MakeSupportBean(String theString, int? intPrimitive)
        {
            return new SupportBean(theString, intPrimitive.Value);
        }

        public static SupportBeanNumeric MakeSupportBeanNumeric(int? intOne, int? intTwo)
        {
            return new SupportBeanNumeric(intOne, intTwo);
        }

        public static object[] FetchObjectArrayEventBean(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            var values = new Dictionary<String, Object>();
            if (id == 0)
            {
                return new Object[2];
            }

            var fields = new Object[2];
            fields[0] = $"|{theString}|";
            fields[1] = id + 1;
            return fields;
        }

        public static IDictionary<string, object> FetchTwoRows3ColsMetadata()
        {
            var values = new LinkedHashMap<String, Object>();
            values.Put("pkey0", typeof(string));
            values.Put("pkey1", typeof(int));
            values.Put("c0", typeof(long));
            return values;
        }

        public static IDictionary<string, object>[] FetchTwoRows3Cols()
        {
            var result = new IDictionary<string, object>[]
            {
                new Dictionary<string, object>(),
                new Dictionary<string, object>()
            };

            result[0].Put("pkey0", "E1");
            result[0].Put("pkey1", 10);
            result[0].Put("c0", 100L);

            result[1].Put("pkey0", "E2");
            result[1].Put("pkey1", 20);
            result[1].Put("c0", 200L);

            return result;
        }

        public static MyMethodReturn[] FetchPonoArray(String mystring, int myint)
        {
            if (myint < 0)
            {
                return null;
            }
            if (myint == 0)
            {
                return new MyMethodReturn[] { new MyMethodReturn(null, null) };
            }
            return new MyMethodReturn[] { new MyMethodReturn("|" + mystring + "|", myint + 1) };
        }

        public static ICollection<MyMethodReturn> FetchPonoCollection(String mystring, int myint)
        {
            if (myint < 0)
            {
                return null;
            }
            if (myint == 0)
            {
                return Collections.SingletonList(new MyMethodReturn(null, null));
            }
            return Collections.SingletonList(new MyMethodReturn("|" + mystring + "|", myint + 1));
        }

        public static IEnumerable<MyMethodReturn> FetchPonoIterable(String mystring, int myint)
        {
            if (myint < 0)
            {
                return null;
            }
            return FetchPonoCollection(mystring, myint);
        }

        public static IEnumerator<MyMethodReturn> FetchPonoIterator(String mystring, int myint)
        {
            if (myint < 0)
            {
                return null;
            }
            return FetchPonoCollection(mystring, myint).GetEnumerator();
        }

        public static MyMethodReturn[] FetchPonoArrayMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return new MyMethodReturn[0];
            }

            var rows = new MyMethodReturn[id];
            for (var i = 0; i < id; i++)
            {
                rows[i] = new MyMethodReturn($"|{theString}_{i}|", i + 100);
            }

            return rows;
        }

        public static ICollection<MyMethodReturn> FetchPonoCollectionMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return Collections.GetEmptyList<MyMethodReturn>();
            }

            var rows = new List<MyMethodReturn>(id);
            for (var i = 0; i < id; i++)
            {
                rows.Add(new MyMethodReturn($"|{theString}_{i}|", i + 100));
            }

            return rows;
        }

        public static IEnumerable<MyMethodReturn> FetchPonoIterableMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchPonoCollectionMR(theString, id);
        }


        public static IEnumerator<MyMethodReturn> FetchPonoIteratorMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchPonoCollectionMR(theString, id).GetEnumerator();
        }

        public static IDictionary<string, object> FetchMapCollectionMRMetadata()
        {
            var values = new Dictionary<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static ICollection<IDictionary<string, object>> FetchMapCollectionMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return Collections.GetEmptyList<IDictionary<string, object>>();
            }

            var rows = new List<IDictionary<string, object>>(id);
            for (var i = 0; i < id; i++)
            {
                var values = new Dictionary<string, object>();
                rows.Add(values);

                values.Put("mapstring", $"|{theString}_{i}|");
                values.Put("mapint", i + 100);
            }

            return rows;
        }

        public static IDictionary<string, object> FetchMapIterableMRMetadata()
        {
            return FetchMapCollectionMRMetadata();
        }

        public static IDictionary<string, object> FetchMapIteratorMRMetadata()
        {
            return FetchMapCollectionMRMetadata();
        }

        public static IEnumerable<IDictionary<string, object>> FetchMapIterableMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchMapCollectionMR(theString, id);
        }

        public static IEnumerator<IDictionary<string, object>> FetchMapIteratorMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchMapCollectionMR(theString, id).GetEnumerator();
        }

        public static IDictionary<string, object> FetchOACollectionMRMetadata()
        {
            var values = new LinkedHashMap<string, object>();
            values.Put("mapstring", typeof(string));
            values.Put("mapint", typeof(int));
            return values;
        }

        public static ICollection<object[]> FetchOACollectionMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (id == 0)
            {
                return Collections.GetEmptyList<object[]>();
            }

            var rows = new List<object[]>(id);
            for (var i = 0; i < id; i++)
            {
                var values = new Object[2];
                rows.Add(values);

                values[0] = $"|{theString}_{i}|";
                values[1] = i + 100;
            }

            return rows;
        }

        public static IDictionary<string, object> FetchOAIterableMRMetadata()
        {
            return FetchOACollectionMRMetadata();
        }

        public static IDictionary<string, object> FetchOAIteratorMRMetadata()
        {
            return FetchOACollectionMRMetadata();
        }

        public static IEnumerable<object[]> FetchOAIterableMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchOACollectionMR(theString, id);
        }

        public static IEnumerator<object[]> FetchOAIteratorMR(String theString, int id)
        {
            if (id < 0)
            {
                return null;
            }
            return FetchOACollectionMR(theString, id).GetEnumerator();
        }

        public class MyMethodReturn
        {
            public MyMethodReturn(String mapstring, int? mapint)
            {
                Mapstring = mapstring;
                Mapint = mapint;
            }

            public string Mapstring { get; private set; }

            public int? Mapint { get; private set; }
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

        public static IDictionary<string, object>[] OverloadedMethodForJoin(string first, int second)
        {
            return GetOverloadedMethodForJoinResult(first, Convert.ToString(second));
        }

        public static IDictionary<string, object>[] OverloadedMethodForJoin(int first, int second)
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

        private static IDictionary<string, object>[] GetOverloadedMethodForJoinResult(String first, String second)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("col1", first);
            values.Put("col2", second);
            return new IDictionary<string, object>[] { values };
        }
    }
}