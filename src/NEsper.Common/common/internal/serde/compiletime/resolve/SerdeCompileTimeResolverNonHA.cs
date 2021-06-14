///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class SerdeCompileTimeResolverNonHA : SerdeCompileTimeResolver
	{
		public static readonly SerdeCompileTimeResolverNonHA INSTANCE = new SerdeCompileTimeResolverNonHA();

		private SerdeCompileTimeResolverNonHA()
		{
		}

		public DataInputOutputSerdeForge SerdeForFilter(
			Type evaluationType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForKeyNonArray(
			Type paramType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge[] SerdeForMultiKey(
			Type[] types,
			StatementRawInfo raw)
		{
			return Noop(types);
		}

		public DataInputOutputSerdeForge[] SerdeForDataWindowSortCriteria(
			Type[] types,
			StatementRawInfo raw)
		{
			return Noop(types);
		}

		public DataInputOutputSerdeForge SerdeForDerivedViewAddProp(
			Type evalType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForBeanEventType(
			StatementRawInfo raw,
			Type underlyingType,
			string eventTypeName,
			IList<EventType> eventTypeSupertypes)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForEventProperty(
			Type typedProperty,
			string eventTypeName,
			string propertyName,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForIndexBtree(
			Type rangeType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForAggregation(
			Type type,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForAggregationDistinct(
			Type type,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForIndexHashNonArray(
			Type propType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForVariable(
			Type type,
			string variableName,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public DataInputOutputSerdeForge SerdeForEventTypeExternalProvider(
			BaseNestableEventType eventType,
			StatementRawInfo raw)
		{
			return Noop();
		}

		public bool IsTargetHA {
			get { return false; }
		}

		private DataInputOutputSerdeForge Noop()
		{
			return DataInputOutputSerdeForgeNotApplicable.INSTANCE;
		}

		private DataInputOutputSerdeForge[] Noop(Type[] types)
		{
			DataInputOutputSerdeForge[] forges = new DataInputOutputSerdeForge[types.Length];
			for (int i = 0; i < forges.Length; i++) {
				forges[i] = Noop();
			}

			return forges;
		}
	}
} // end of namespace
