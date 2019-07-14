///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
    public class LambdaAssertionUtil
    {
        public static void AssertValuesArrayScalar(
            SupportListener listener,
            string field,
            params object[] expected)
        {
            var result = listener.AssertOneGetNew().Get(field);
            if (expected == null) {
                Assert.IsNull(result);
                return;
            }

            var arr = ((ICollection<object>) result).ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(expected, arr);
        }

        public static void AssertST0Id(
            SupportListener listener,
            string property,
            string expectedList)
        {
            var arr = listener.AssertOneGetNew()
                .Get(property)
                .Unwrap<SupportBean_ST0>()
                .ToArray();
            if (expectedList.Length == 0 && arr.Length == 0) {
                return;
            }

            var expected = expectedList.SplitCsv();
            Assert.AreEqual(expected.Length, arr.Length, "Received: " + GetIds(arr));
            for (var i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], arr[i].Id);
            }
        }

        public static string GetIds(SupportBean_ST0[] arr)
        {
            var delimiter = "";
            var writer = new StringBuilder();
            foreach (var item in arr) {
                writer.Append(delimiter);
                delimiter = ",";
                writer.Append(item.Id);
            }

            return writer.ToString();
        }

        private static SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it)
        {
            if (it == null) {
                return null;
            }

            if (it.IsEmpty()) {
                return new SupportBean_ST0[0];
            }

            return it.ToArray();
        }

        public static void AssertTypes(
            EventType type,
            string[] fields,
            Type[] classes)
        {
            var count = 0;
            foreach (var field in fields) {
                Assert.AreEqual(classes[count++], type.GetPropertyType(field), "position " + count);
            }
        }

        public static void AssertTypesAllSame(
            EventType type,
            string[] fields,
            Type clazz)
        {
            var count = 0;
            foreach (var field in fields) {
                Assert.AreEqual(clazz, type.GetPropertyType(field), "position " + count);
            }
        }

        public static void AssertSingleAndEmptySupportColl(
            RegressionEnvironment env,
            string[] fields)
        {
            env.SendEventBean(SupportCollection.MakeString("E1"));
            foreach (var field in fields) {
                AssertValuesArrayScalar(env.Listener("s0"), field, "E1");
            }

            env.Listener("s0").Reset();

            env.SendEventBean(SupportCollection.MakeString(null));
            foreach (var field in fields) {
                AssertValuesArrayScalar(env.Listener("s0"), field, null);
            }

            env.Listener("s0").Reset();

            env.SendEventBean(SupportCollection.MakeString(""));
            foreach (var field in fields) {
                AssertValuesArrayScalar(env.Listener("s0"), field);
            }

            env.Listener("s0").Reset();
        }
    }
} // end of namespace