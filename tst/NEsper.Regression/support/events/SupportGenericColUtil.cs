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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.events
{
	public class SupportGenericColUtil
	{
		public static readonly PairOfNameAndType[] NAMESANDTYPES = new PairOfNameAndType[] {
			FromPair("listOfString", "System.Collections.Generic.IList<System.String>", typeof(IList<string>)),
			FromPair("listOfOptionalInteger", "System.Collections.Generic.IList<Nullable<Integer>>", typeof(IList<int?>)),
			FromPair("mapOfStringAndInteger", "System.Collections.Generic.IDictionary<String, Integer>", typeof(IDictionary<string, int?>)),
			FromPair("listArrayOfString", "System.Collections.Generic.IList<String>[]", typeof(IList<string>[])),
			FromPair("listOfStringArray", "System.Collections.Generic.IList<String[]>", typeof(IList<string[]>)),
			FromPair("listArray2DimOfString", "System.Collections.Generic.IList<String>[][]", typeof(IList<string>[][])),
			FromPair("listOfStringArray2Dim", "System.Collections.Generic.IList<String[][]>", typeof(IList<string[][]>)),
			FromPair("listOfT", "System.Collections.Generic.IList<Object>", typeof(IList<object>))
		};

		public static string AllNames()
		{
			var names = new StringBuilder();
			var delimiter = "";
			foreach (var pair in NAMESANDTYPES) {
				names
					.Append(delimiter)
					.Append(pair.Name);
				delimiter = ",";
			}

			return names.ToString();
		}

		public static string AllNamesAndTypes()
		{
			var names = new StringBuilder();
			var delimiter = "";
			foreach (var pair in NAMESANDTYPES) {
				names
					.Append(delimiter)
					.Append(pair.Name)
					.Append(" ")
					.Append(pair.TypeName);
				delimiter = ",";
			}

			return names.ToString();
		}

		public static void AssertPropertyTypes(EventType type)
		{
			SupportEventPropUtil.AssertPropsEquals(
				type.PropertyDescriptors,
				new SupportEventPropDesc("listOfString", typeof(IList<string>)),
				new SupportEventPropDesc("listOfOptionalInteger", typeof(IList<int?>)),
				new SupportEventPropDesc("mapOfStringAndInteger", typeof(IDictionary<string, int?>)),
				new SupportEventPropDesc("listArrayOfString", typeof(IList<string>[])),
				new SupportEventPropDesc("listOfStringArray", typeof(IList<string[]>)),
				new SupportEventPropDesc("listArray2DimOfString", typeof(IList<string>[][])),
				new SupportEventPropDesc("listOfStringArray2Dim", typeof(IList<string[][]>)),
				new SupportEventPropDesc("listOfT", typeof(IList<object>))
			);
		}

		public static IDictionary<string, object> SampleEvent {
			get {
				IDictionary<string, object> fields = new Dictionary<string, object>();
				fields.Put("listOfString", MakeListOfString());
				fields.Put("listOfOptionalInteger", MakeListOfNullableInteger());
				fields.Put("mapOfStringAndInteger", MakeMapOfStringAndInteger());
				fields.Put("listArrayOfString", MakeListArrayOfString());
				fields.Put("listOfStringArray", MakeListOfStringArray());
				fields.Put("listArray2DimOfString", MakeListArray2DimOfString());
				fields.Put("listOfStringArray2Dim", MakeListOfStringArray2Dim());
				fields.Put("listOfT", MakeListOfT());
				return fields;
			}
		}

		public static void Compare(EventBean @event)
		{
			Assert.AreEqual(MakeListOfString(), @event.Get("listOfString"));
			Assert.AreEqual(MakeListOfNullableInteger(), @event.Get("listOfOptionalInteger"));
			Assert.AreEqual(MakeMapOfStringAndInteger(), @event.Get("mapOfStringAndInteger"));
			Assert.AreEqual(MakeListArrayOfString().RenderAny(), @event.Get("listArrayOfString").UnwrapIntoList<string[]>().RenderAny());
			EPAssertionUtil.AssertEqualsExactOrder(MakeListOfStringArray().ToArray(), @event.Get("listOfStringArray").UnwrapIntoArray<string[]>());
			Assert.AreEqual(MakeListArray2DimOfString()[0].RenderAny(), @event.Get("listArray2DimOfString").UnwrapIntoArray<string[][]>()[0].RenderAny());
			EPAssertionUtil.AssertEqualsExactOrder(MakeListOfStringArray2Dim().ToArray(), @event.Get("listOfStringArray2Dim").UnwrapIntoArray<string[][]>());
			EPAssertionUtil.AssertEqualsExactOrder(MakeListOfT().ToArray(), @event.Get("listOfT").UnwrapIntoArray<object>());
		}

		private static IList<string> MakeListOfString()
		{
			return Arrays.AsList("a");
		}

		private static IList<int?> MakeListOfNullableInteger()
		{
			return Arrays.AsList<int?>(10);
		}

		private static IDictionary<string, int?> MakeMapOfStringAndInteger()
		{
			return Collections.SingletonMap<string, int?>("k", 20);
		}

		private static IList<string>[] MakeListArrayOfString()
		{
			return new[] { Arrays.AsList("b") };
		}

		private static IList<string[]> MakeListOfStringArray()
		{
			IList<string[]> list = new List<string[]>();
			list.Add(new[] {"c"});
			return list;
		}

		private static IList<string>[][] MakeListArray2DimOfString()
		{
			return new[] {
				new IList<string>[] {
					new[] { "b" }
				}
			};
		}

		private static IList<string[][]> MakeListOfStringArray2Dim()
		{
			IList<string[][]> list = new List<string[][]>();
			list.Add(new string[][] { new string[]{"c"}});
			return list;
		}

		private static IList<object> MakeListOfT()
		{
			return Collections.SingletonList<object>("x");
		}

		private static PairOfNameAndType FromPair(
			string name,
			string type,
			Type typeClass)
		{
			return new PairOfNameAndType(name, type, typeClass);
		}

		public class PairOfNameAndType
		{
			public PairOfNameAndType(
				string name,
				string typeName,
				Type typeClass)
			{
				this.Name = name;
				this.TypeName = typeName;
				this.TypeClass = typeClass;
			}

			public string Name { get; }

			public string TypeName { get; }

			public Type TypeClass { get; }
		}
	}
} // end of namespace
