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
using System.Text;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.json
{
	public class SupportJsonEventTypeUtil
	{
		public static Type GetUnderlyingType(
			RegressionEnvironment env,
			string statementNameOfDeployment,
			string typeName)
		{
			string deploymentId = env.DeploymentId(statementNameOfDeployment);
			if (deploymentId == null) {
				throw new ArgumentException("Failed to find deployment id for statement '" + statementNameOfDeployment + "'");
			}

			EventType eventType = env.Runtime.EventTypeService.GetEventType(deploymentId, typeName);
			if (eventType == null) {
				throw new ArgumentException("Failed to find event type '" + typeName + "' for deployment '" + deploymentId + "'");
			}

			return eventType.UnderlyingType;
		}

		public static Type GetNestedUnderlyingType(
			JsonEventType eventType,
			string propertyName)
		{
			object type = eventType.Types.Get(propertyName);
			EventType innerType;
			if (type is TypeBeanOrUnderlying) {
				innerType = ((TypeBeanOrUnderlying) type).EventType;
			}
			else {
				innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
			}

			return innerType.UnderlyingType;
		}

		public static void AssertJsonWrite(
			string jsonExpected,
			EventBean eventBean)
		{
			var textReader = new StringReader(jsonExpected);
			var jsonReader = new JsonTextReader(textReader);
			var jsonValue = JToken.ReadFrom(jsonReader);
			AssertJsonWrite(jsonValue, eventBean);
		}

		public static void AssertJsonWrite(
			JToken expectedValue,
			EventBean eventBean)
		{
			var expectedMinimalJson = expectedValue.ToString(Formatting.None);
			var expectedPrettyJson = expectedValue.ToString(Formatting.Indented);

			var optionsMinimal = new JsonWriterOptions();
			var optionsIndent = new JsonWriterOptions() { Indented = true };
			
			var und = (JsonEventObject) eventBean.Underlying;
			Assert.AreEqual(expectedMinimalJson, und.ToString(optionsMinimal));
			Assert.AreEqual(expectedPrettyJson, und.ToString(optionsIndent));

			var stream = new MemoryStream();
			var writer = new Utf8JsonWriter(stream, optionsMinimal);
			var context = new JsonSerializationContext(writer);
			
			und.WriteTo(context);

			Assert.AreEqual(expectedMinimalJson, Encoding.UTF8.GetString(stream.ToArray()));
		}

		public static void CompareDictionaries(
			IDictionary<string, object> expected,
			IDictionary<string, object> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);
			Assert.AreEqual(expected.IsEmpty(), actual.IsEmpty());

			CompareCollection(expected.Keys, actual.Keys, "DUMMY", false);
			CompareCollection(expected.Values, actual.Values, "DUMMY", false);
			CompareCollection(expected, actual, new KeyValuePair<string, object>("DUMMY", "DUMMY-VALUE"), true);
			
			// At this point, we've verified that the keys are the same, the values are the same, and the
			// actual entries of the map are identical.  We can compare individual keys -> value return values
			// but it is fruitless since the set only allows a single key-value to be mapped.  As such, the
			// collection comparison for the set of key-value pairs is sufficient.
			
			Assert.That(actual.ContainsKey("DUMMY"), Is.False);
			Assert.That(expected.ContainsKey("DUMMY"), Is.False);
		}

		private static void CompareCollection<T>(
			ICollection<T> expected,
			ICollection<T> actual,
			T dummyValue,
			bool allowMutability)
		{
			// First verify that they are equivalent.  Order is not important in equivalence.
			CollectionAssert.AreEquivalent(expected, actual);
			// Next verify equality.  Equality requires complete consistency, including order.
			CollectionAssert.AreEqual(expected, actual);
			// Collection may return mutable or immutable.  If it reports readonly, it must
			// adhere to strict immutability.
			if (!allowMutability) {
				Assert.IsTrue(actual.IsReadOnly);
			}

			// Assert containment
			Assert.IsTrue(actual.ContainsAll(expected));
			// Assert non-containment for something that should not be there.
			Assert.IsFalse(actual.Contains(dummyValue));
			Assert.IsFalse(actual.ContainsAll(Arrays.AsList(dummyValue)));
			// these operations should be fail due to immutability of the keyset
			if (actual.IsReadOnly) {
				Assert.Throws<NotSupportedException>(actual.Clear);
				Assert.Throws<NotSupportedException>(() => actual.Remove(dummyValue));
				Assert.Throws<NotSupportedException>(() => actual.Add(dummyValue));
			}
		}

		public static bool IsBeanBackedJson(EventType eventType)
		{
			if (eventType is JsonEventType jsonEventType) {
				return jsonEventType.Detail.OptionalUnderlyingProvided != null;
			}

			return false;
		}
	}
} // end of namespace
