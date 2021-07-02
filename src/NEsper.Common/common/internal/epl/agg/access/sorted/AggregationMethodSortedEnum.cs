///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public enum AggregationMethodSortedEnum
	{
		FLOOREVENT,
		FLOOREVENTS,
		FLOORKEY,
		CEILINGEVENT,
		CEILINGEVENTS,
		CEILINGKEY,
		LOWEREVENT,
		LOWEREVENTS,
		LOWERKEY,
		HIGHEREVENT,
		HIGHEREVENTS,
		HIGHERKEY,

		FIRSTEVENT,
		FIRSTEVENTS,
		FIRSTKEY,
		LASTEVENT,
		LASTEVENTS,
		LASTKEY,

		GETEVENT,
		GETEVENTS,
		CONTAINSKEY,
		COUNTEVENTS,
		COUNTKEYS,

		EVENTSBETWEEN,
		SUBMAP,
		DICTIONARYREFERENCE,
	}

	public static class AggregationMethodSortedEnumExtensions
	{
		public static AggregationMethodSortedEnum? FromString(string nameMixed)
		{
			try {
				return EnumHelper.Parse<AggregationMethodSortedEnum>(nameMixed);
			}
			catch (ArgumentException) {
				return null;
			}
		}

		public static bool IsReturnsCollectionOfEvents(this AggregationMethodSortedEnum value)
		{
			return value switch {
				AggregationMethodSortedEnum.FLOOREVENT => (false),
				AggregationMethodSortedEnum.FLOOREVENTS => (true),
				AggregationMethodSortedEnum.FLOORKEY => (false),
				AggregationMethodSortedEnum.CEILINGEVENT => (false),
				AggregationMethodSortedEnum.CEILINGEVENTS => (true),
				AggregationMethodSortedEnum.CEILINGKEY => (false),
				AggregationMethodSortedEnum.LOWEREVENT => (false),
				AggregationMethodSortedEnum.LOWEREVENTS => (true),
				AggregationMethodSortedEnum.LOWERKEY => (false),
				AggregationMethodSortedEnum.HIGHEREVENT => (false),
				AggregationMethodSortedEnum.HIGHEREVENTS => (true),
				AggregationMethodSortedEnum.HIGHERKEY => (false),
				AggregationMethodSortedEnum.FIRSTEVENT => (false),
				AggregationMethodSortedEnum.FIRSTEVENTS => (true),
				AggregationMethodSortedEnum.FIRSTKEY => (false),
				AggregationMethodSortedEnum.LASTEVENT => (false),
				AggregationMethodSortedEnum.LASTEVENTS => (true),
				AggregationMethodSortedEnum.LASTKEY => (false),
				AggregationMethodSortedEnum.GETEVENT => (false),
				AggregationMethodSortedEnum.GETEVENTS => (true),
				AggregationMethodSortedEnum.CONTAINSKEY => (false),
				AggregationMethodSortedEnum.COUNTEVENTS => (false),
				AggregationMethodSortedEnum.COUNTKEYS => (false),
				AggregationMethodSortedEnum.EVENTSBETWEEN => (true),
				AggregationMethodSortedEnum.SUBMAP => (false),
				AggregationMethodSortedEnum.DICTIONARYREFERENCE => (false),
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}

		public static bool IsReturnsSingleEvent(this AggregationMethodSortedEnum value)
		{
			return value switch {
				AggregationMethodSortedEnum.FLOOREVENT => true,
				AggregationMethodSortedEnum.FLOOREVENTS => false,
				AggregationMethodSortedEnum.FLOORKEY => false,
				AggregationMethodSortedEnum.CEILINGEVENT => true,
				AggregationMethodSortedEnum.CEILINGEVENTS => false,
				AggregationMethodSortedEnum.CEILINGKEY => false,
				AggregationMethodSortedEnum.LOWEREVENT => true,
				AggregationMethodSortedEnum.LOWEREVENTS => false,
				AggregationMethodSortedEnum.LOWERKEY => false,
				AggregationMethodSortedEnum.HIGHEREVENT => true,
				AggregationMethodSortedEnum.HIGHEREVENTS => false,
				AggregationMethodSortedEnum.HIGHERKEY => false,
				AggregationMethodSortedEnum.FIRSTEVENT => true,
				AggregationMethodSortedEnum.FIRSTEVENTS => false,
				AggregationMethodSortedEnum.FIRSTKEY => false,
				AggregationMethodSortedEnum.LASTEVENT => true,
				AggregationMethodSortedEnum.LASTEVENTS => false,
				AggregationMethodSortedEnum.LASTKEY => false,
				AggregationMethodSortedEnum.GETEVENT => true,
				AggregationMethodSortedEnum.GETEVENTS => false,
				AggregationMethodSortedEnum.CONTAINSKEY => false,
				AggregationMethodSortedEnum.COUNTEVENTS => false,
				AggregationMethodSortedEnum.COUNTKEYS => false,
				AggregationMethodSortedEnum.EVENTSBETWEEN => false,
				AggregationMethodSortedEnum.SUBMAP => false,
				AggregationMethodSortedEnum.DICTIONARYREFERENCE => false,
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}

		public static AggregationMethodSortedFootprintEnum GetFootprint(this AggregationMethodSortedEnum value)
		{
			return value switch {
				AggregationMethodSortedEnum.FLOOREVENT => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.FLOOREVENTS => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.FLOORKEY => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.CEILINGEVENT => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.CEILINGEVENTS => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.CEILINGKEY => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.LOWEREVENT => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.LOWEREVENTS => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.LOWERKEY => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.HIGHEREVENT => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.HIGHEREVENTS => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.HIGHERKEY => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.FIRSTEVENT => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.FIRSTEVENTS => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.FIRSTKEY => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.LASTEVENT => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.LASTEVENTS => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.LASTKEY => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.GETEVENT => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.GETEVENTS => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.CONTAINSKEY => AggregationMethodSortedFootprintEnum.KEYONLY,
				AggregationMethodSortedEnum.COUNTEVENTS => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.COUNTKEYS => AggregationMethodSortedFootprintEnum.NOPARAM,
				AggregationMethodSortedEnum.EVENTSBETWEEN => AggregationMethodSortedFootprintEnum.SUBMAP,
				AggregationMethodSortedEnum.SUBMAP => AggregationMethodSortedFootprintEnum.SUBMAP,
				AggregationMethodSortedEnum.DICTIONARYREFERENCE => AggregationMethodSortedFootprintEnum.NOPARAM,
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}

		public static Type GetResultType(
			this AggregationMethodSortedEnum value,
			Type underlyingType,
			Type keyType)
		{
			switch (value) {
				case AggregationMethodSortedEnum.CONTAINSKEY:
					return typeof(bool);

				case AggregationMethodSortedEnum.COUNTEVENTS:
				case AggregationMethodSortedEnum.COUNTKEYS:
					return typeof(int);

				case AggregationMethodSortedEnum.SUBMAP:
				case AggregationMethodSortedEnum.DICTIONARYREFERENCE:
					return typeof(IOrderedDictionary<object, EventBean>);
			}

			if (!IsReturnsSingleEvent(value) && !IsReturnsCollectionOfEvents(value)) {
				return keyType;
			}

			if (IsReturnsSingleEvent(value)) {
				return underlyingType;
			}

			if (IsReturnsCollectionOfEvents(value)) {
				return TypeHelper.GetArrayType(underlyingType);
			}

			throw new UnsupportedOperationException("Unrecognized type for " + value);
		}
	}
} // end of namespace
