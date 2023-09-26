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

using Microsoft.CodeAnalysis;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.events
{
	public class SupportGenericColUtil
	{
		public static readonly PairOfNameAndType[] NAMESANDTYPES = new PairOfNameAndType[] {
			FromPair("listOfString", "System.Collections.Generic.IList<String>", typeof(IList<string>)),
			FromPair(
				"listOfOptionalInteger",
				"System.Collections.Generic.IList<Optional<Integer>>",
				typeof(IList<Optional<int?>>)),
			FromPair(
				"mapOfStringAndInteger",
				"System.Collections.Generic.IDictionary<String, Integer>",
				typeof(IDictionary<string, int?>)),
			FromPair("listArrayOfString", "System.Collections.Generic.IList<String>[]", typeof(IList<string>[])),
			FromPair("listOfStringArray", "System.Collections.Generic.IList<String[]>", typeof(IList<string[]>)),
			FromPair(
				"listArray2DimOfString",
				"System.Collections.Generic.IList<String>[][]",
				typeof(IList<string>[][])),
			FromPair(
				"listOfStringArray2Dim",
				"System.Collections.Generic.IList<String[][]>",
				typeof(IList<string[][]>)),
			FromPair("listOfT", "System.Collections.Generic.IList<Object>", typeof(IList<object>))
		};

		public static string AllNames()
		{
			var names = new StringBuilder();
			var delimiter = "";
			foreach (var pair in NAMESANDTYPES) {
				names.Append(delimiter).Append(pair.Name);
				delimiter = ",";
			}

			return names.ToString();
		}

		public static string AllNamesAndTypes()
		{
			var names = new StringBuilder();
			var delimiter = "";
			foreach (var pair in NAMESANDTYPES) {
				names.Append(delimiter).Append(pair.Name).Append(" ").Append(pair.TypeName);
				delimiter = ",";
			}

			return names.ToString();
		}

		public static void AssertPropertyTypes(EventType type)
		{
			SupportEventPropUtil.AssertPropsEquals(
				type.PropertyDescriptors.ToArray(),
				new SupportEventPropDesc("listOfString", typeof(IList<string>)),
				new SupportEventPropDesc("listOfOptionalInteger", typeof(IList<int?>)),
				new SupportEventPropDesc("mapOfStringAndInteger", typeof(IDictionary<string, int>)),
				new SupportEventPropDesc("listArrayOfString", typeof(IList<string>[])),
				new SupportEventPropDesc("listOfStringArray", typeof(IList<string[]>)),
				new SupportEventPropDesc("listArray2DimOfString", typeof(IList<string>[][])),
				new SupportEventPropDesc("listOfStringArray2Dim", typeof(IList<string[][]>)),
				new SupportEventPropDesc("listOfT", typeof(IList<object>))
			);
		}

		public static IDictionary<string, object> GetSampleEvent()
		{
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

		public static void Compare(EventBean @event)
		{
			Assert.AreEqual(MakeListOfString(), @event.Get("listOfString"));
			Assert.AreEqual(MakeListOfNullableInteger(), @event.Get("listOfOptionalInteger"));
			Assert.AreEqual(MakeMapOfStringAndInteger(), @event.Get("mapOfStringAndInteger"));
			Assert.AreEqual(
				MakeListArrayOfString().Render(),
				@event.Get("listArrayOfString").UnwrapIntoList<string[]>().RenderAny());
			EPAssertionUtil.AssertEqualsExactOrder(
				MakeListOfStringArray().ToArray(),
				((IList<string[]>)@event.Get("listOfStringArray")).ToArray());
			Assert.AreEqual(
				MakeListArray2DimOfString()[0].RenderAny(),
				((IList<string>[][])@event.Get("listArray2DimOfString"))[0].RenderAny());
			EPAssertionUtil.AssertEqualsExactOrder(
				MakeListOfStringArray2Dim().ToArray(),
				((IList<string[][]>)@event.Get("listOfStringArray2Dim")).ToArray());
			EPAssertionUtil.AssertEqualsExactOrder(
				MakeListOfT().ToArray(),
				((IList<object>)@event.Get("listOfT")).ToArray());
		}

		private static IList<string> MakeListOfString()
		{
			return Arrays.AsList("a");
		}

		private static IList<int?> MakeListOfNullableInteger()
		{
			return Arrays.AsList<int?>(10);
		}

		private static IDictionary<string, int> MakeMapOfStringAndInteger()
		{
			return Collections.SingletonMap("k", 20);
		}

		private static IList<string>[] MakeListArrayOfString()
		{
			return new IList<string>[] { Arrays.AsList("b") };
		}

		private static IList<string[]> MakeListOfStringArray()
		{
			IList<string[]> list = new List<string[]>();
			list.Add(new string[] { "c" });
			return list;
		}

		private static IList<string>[][] MakeListArray2DimOfString()
		{
			return new IList<string>[][] { new IList<string>[] { Arrays.AsList("b") } };
		}

		private static IList<string[][]> MakeListOfStringArray2Dim()
		{
			IList<string[][]> list = new List<string[][]>();
			list.Add(new string[][] { new string[] { "c" } });
			return list;
		}

		private static IList<object> MakeListOfT()
		{
			return Arrays.AsList<object>("x");
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
			private readonly string name;
			private readonly string type;
			private readonly Type typeClass;

			public PairOfNameAndType(
				string name,
				string type,
				Type typeClass)
			{
				this.name = name;
				this.type = type;
				this.typeClass = typeClass;
			}

			public string Name => name;

			public string TypeName => type;

			public Type TypeClass => typeClass;
		}
	}
} // end of namespace
