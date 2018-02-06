///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.bean.lambda
{
    public class LambdaAssertionUtil
    {
        public static void AssertValuesArrayScalar(SupportUpdateListener listener, String field, params object[] expected)
        {
            if (expected == null)
            {
                Assert.IsNull(listener.AssertOneGetNew().Get(field));
                return;
            }
            object[] arr = listener.AssertOneGetNew().Get(field).UnwrapIntoArray<object>();
            EPAssertionUtil.AssertEqualsExactOrder(expected, arr);
        }

        public static void AssertST0Id(SupportUpdateListener listener, String property, String expectedList)
        {
            SupportBean_ST0[] arr = listener.AssertOneGetNew().Get(property).UnwrapIntoArray<SupportBean_ST0>();
            if (expectedList == null && arr == null)
            {
                return;
            }
            if ((expectedList.Length == 0) && (arr.Length == 0))
            {
                return;
            }
            String[] expected = expectedList.Split(',');
            Assert.AreEqual(expected.Length, arr.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], arr[i].Id);
            }
        }

        private static SupportBean_ST0[] ToArray(IEnumerable<object> it)
        {
            if (it == null)
                return null;
            if (!it.HasFirst())
                return new SupportBean_ST0[0];

            return it.Cast<SupportBean_ST0>().ToArray();
        }

        private static SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it)
        {
            if (it == null)
            {
                return null;
            }
            if (it.IsEmpty())
            {
                return new SupportBean_ST0[0];
            }
            return it.ToArray();
        }

        public static void AssertTypes(EventType type, String[] fields, Type[] classes)
        {
            for (int ii = 0; ii < fields.Length; ii++)
            {
                Assert.That(type.GetPropertyType(fields[ii]), Is.EqualTo(classes[ii]), "position " + ii);
            }
        }

        public static void AssertTypesAllSame(EventType type, String[] fields, Type clazz)
        {
            int count = 0;
            foreach (var field in fields)
            {
                Assert.That(clazz, Is.EqualTo(type.GetPropertyType(field)), "position " + count);
            }
        }

        public static void AssertSingleAndEmptySupportColl(EPServiceProvider epService, SupportUpdateListener listener, String[] fields)
        {
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            foreach (string field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field, "E1");
            }
            listener.Reset();

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            foreach (string field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field, null);
            }
            listener.Reset();

            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            foreach (string field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field);
            }
            listener.Reset();
        }
    }
}
