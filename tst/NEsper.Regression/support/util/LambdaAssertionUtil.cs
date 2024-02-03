///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.util
{
	public class LambdaAssertionUtil
	{

		public static void AssertValuesArrayScalar(
			RegressionEnvironment env,
			string field,
			params object[] expected)
		{
			env.AssertListener(
				"s0",
				listener => {
					var result = listener.AssertOneGetNew().Get(field);
					AssertValuesArrayScalar(result, expected);
				});
		}

		public static void AssertValuesArrayScalarWReset(
			RegressionEnvironment env,
			string field,
			params object[] expected)
		{
			env.AssertEventNew("s0", @event => AssertValuesArrayScalar(@event.Get(field), expected));
		}

		public static void AssertValuesArrayScalar(
			EventBean @event,
			string field,
			params object[] expected)
		{
			var result = @event.Get(field);
			AssertValuesArrayScalar(result, expected);
		}

		public static void AssertValuesArrayScalar(
			object result,
			params object[] expected)
		{
			if (expected == null) {
				ClassicAssert.IsNull(result);
				return;
			}

			var arr = result.UnwrapIntoArray<object>();
			EPAssertionUtil.AssertEqualsExactOrder(expected, arr);
		}

		public static void AssertST0IdWReset(
			RegressionEnvironment env,
			string property,
			string expectedList)
		{
			env.AssertEventNew("s0", @event => AssertST0Id(@event, property, expectedList));
		}

		public static void AssertST0Id(
			RegressionEnvironment env,
			string property,
			string expectedList)
		{
			env.AssertListener("s0", listener => { AssertST0Id(listener.AssertOneGetNew(), property, expectedList); });
		}

		private static void AssertST0Id(
			EventBean eventBean,
			string property,
			string expectedList)
		{
			AssertST0Id(eventBean.Get(property), expectedList);
		}

		public static void AssertST0Id(
			object value,
			string expectedList)
		{
			var arr = value.UnwrapIntoArray<SupportBean_ST0>();
			if (expectedList == null && arr == null) {
				return;
			}

			if (string.IsNullOrEmpty(expectedList) && arr.Length == 0) {
				return;
			}

			var expected = expectedList.SplitCsv();
			ClassicAssert.AreEqual(expected.Length, arr.Length, "Received: " + GetIds(arr));
			for (var i = 0; i < expected.Length; i++) {
				ClassicAssert.AreEqual(expected[i], arr[i].Id);
			}
		}

		public static string GetIds(SupportBean_ST0[] arr)
		{
			var delimiter = "";
			var writer = new StringWriter();
			foreach (var item in arr) {
				writer.Write(delimiter);
				delimiter = ",";
				writer.Write(item.Id);
			}

			return writer.ToString();
		}

		private static SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it)
		{
			if (it == null) {
				return null;
			}

			if (it.IsEmpty()) {
				return Array.Empty<SupportBean_ST0>();
			}

			return it.ToArray();
		}

		public static void AssertSingleAndEmptySupportColl(
			SupportEvalBuilder builder,
			string[] fields)
		{
			var assertionOne = builder.WithAssertion(SupportCollection.MakeString("E1"));
			foreach (var field in fields) {
				assertionOne.Verify(field, value => AssertValuesArrayScalar(value, "E1"));
			}

			var assertionTwo = builder.WithAssertion(SupportCollection.MakeString(null));
			foreach (var field in fields) {
				assertionTwo.Verify(field, value => AssertValuesArrayScalar(value, null));
			}

			var assertionThree = builder.WithAssertion(SupportCollection.MakeString(""));
			foreach (var field in fields) {
				assertionThree.Verify(field, value => AssertValuesArrayScalar(value));
			}
		}
	}
} // end of namespace
