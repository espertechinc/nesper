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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportFilterSpecBuilder
    {
        public static FilterSpecActivatable Build(
            EventType eventType,
            object[] objects)
        {
            FilterSpecParam[][] @params = { BuildList(eventType, objects) };
            return new FilterSpecActivatable(eventType, "SomeAliasNameForType", @params, null, 1);
        }

        public static FilterSpecParam[] BuildList(
            EventType eventType,
            object[] objects)
        {
            IList<FilterSpecParam> filterParams = new List<FilterSpecParam>();

            var index = 0;
            while (objects.Length > index)
            {
                var propertyName = (string) objects[index++];
                var filterOperator = (FilterOperator) objects[index++];

                if (!filterOperator.IsRangeOperator())
                {
                    var filterForConstant = objects[index++];
                    filterParams.Add(new SupportFilterSpecParamConstant(
                        MakeLookupable(eventType, propertyName), filterOperator, filterForConstant));
                }
                else
                {
                    var min = objects[index++].AsDouble();
                    var max = objects[index++].AsDouble();
                    filterParams.Add(
                        new SupportFilterSpecParamRange(
                            MakeLookupable(eventType, propertyName),
                            filterOperator,
                            new SupportFilterForEvalConstantDouble(min),
                            new SupportFilterForEvalConstantDouble(max)));
                }
            }

            return filterParams.ToArray();
        }

        private static ExprFilterSpecLookupable MakeLookupable(
            EventType eventType,
            string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, eventType.GetGetter(fieldName), eventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace
