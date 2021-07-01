///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.support;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
	public class SupportFilterSpecBuilder
	{
		public static FilterSpecActivatable Build(
			EventType eventType,
			object[] objects)
		{
			var triplets = BuildTriplets(eventType, objects);
			var paths = new FilterSpecPlanPath[] {new FilterSpecPlanPath(triplets)};
			var plan = new FilterSpecPlan(paths, null, null);
			plan.Initialize();
			return new FilterSpecActivatable(eventType, "SomeAliasNameForType", plan, null, 1);
		}

		public static FilterSpecPlanPathTriplet[] BuildTriplets(
			EventType eventType,
			object[] objects)
		{
			IList<FilterSpecPlanPathTriplet> filterParams = new List<FilterSpecPlanPathTriplet>();

			var index = 0;
			while (objects.Length > index) {
				var propertyName = (string) objects[index++];
				var filterOperator = (FilterOperator) objects[index++];

				if (!(filterOperator.IsRangeOperator())) {
					var filterForConstant = objects[index++];
					FilterSpecParam param = new SupportFilterSpecParamConstant(MakeLookupable(eventType, propertyName), filterOperator, filterForConstant);
					filterParams.Add(new FilterSpecPlanPathTriplet(param));
				}
				else {
					var min = objects[index++].AsDouble();
					var max = objects[index++].AsDouble();
					FilterSpecParam param = new SupportFilterSpecParamRange(
						MakeLookupable(eventType, propertyName),
						filterOperator,
						new SupportFilterForEvalConstantDouble(min),
						new SupportFilterForEvalConstantDouble(max));
					filterParams.Add(new FilterSpecPlanPathTriplet(param));
				}
			}

			return filterParams.ToArray();
		}

		private static ExprFilterSpecLookupable MakeLookupable(
			EventType eventType,
			string fieldName)
		{
			var eval = new SupportExprEventEvaluator(eventType.GetGetter(fieldName));
			return new ExprFilterSpecLookupable(fieldName, eval, null, eventType.GetPropertyType(fieldName), false, null);
		}
	}

} // end of namespace
