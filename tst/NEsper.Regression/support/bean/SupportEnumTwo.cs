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

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
	public enum SupportEnumTwo
	{
		ENUM_VALUE_1,
		ENUM_VALUE_2,
		ENUM_VALUE_3
	}

	public static class SupportEnumTwoExtensions
	{
		public static int GetAssociatedValue(this SupportEnumTwo value)
		{
			return value switch {
				SupportEnumTwo.ENUM_VALUE_1 => 100,
				SupportEnumTwo.ENUM_VALUE_2 => 200,
				SupportEnumTwo.ENUM_VALUE_3 => 300,
				_ => throw new ArgumentException(nameof(value))
			};
		}

		public static string[] GetMystrings(this SupportEnumTwo value)
		{
			return value switch {
				SupportEnumTwo.ENUM_VALUE_1 => new[] { "1", "0", "0" },
				SupportEnumTwo.ENUM_VALUE_2 => new[] { "2", "0", "0" },
				SupportEnumTwo.ENUM_VALUE_3 => new[] { "3", "0", "0" },
				_ => throw new ArgumentException(nameof(value))
			};
		}

		public static bool CheckAssociatedValue(
			this SupportEnumTwo enumValue,
			int value)
		{
			return GetAssociatedValue(enumValue) == value;
		}

		public static bool CheckEventBeanPropInt(
			this SupportEnumTwo enumValue,
			EventBean @event,
			string propertyName)
		{
			var value = @event.Get(propertyName);
			if (value is int intValue) {
				return GetAssociatedValue(enumValue) == intValue;
			}

			return false;
		}

		public static Nested GetNested(this SupportEnumTwo enumValue)
		{
			return new Nested(GetAssociatedValue(enumValue), GetMystrings(enumValue));
		}
		
		public static IList<string> GetMyStringsAsList(this SupportEnumTwo value)
		{
			return GetMystrings(value).ToList();
		}


		public class Nested
		{
			private readonly int value;
			private readonly string[] mystrings;

			public Nested(
				int value,
				string[] mystrings)
			{
				this.value = value;
				this.mystrings = mystrings;
			}

			public int Value => value;

			public int GetValue()
			{
				return Value;
			}

			public string[] Mystrings => mystrings;

			public string[] GetMystrings()
			{
				return mystrings;
			}

			public IList<string> GetMyStringsNestedAsList()
			{
				return Arrays.AsList(mystrings);
			}
		}
	}
} // end of namespace
