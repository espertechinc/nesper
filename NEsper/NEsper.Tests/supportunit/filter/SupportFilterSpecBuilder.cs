///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.filter;

namespace com.espertech.esper.supportunit.filter
{
    public class SupportFilterSpecBuilder
    {
        public static FilterSpecCompiled Build(EventType eventType, Object[] objects)
        {
            return new FilterSpecCompiled(eventType, "SomeAliasNameForType", new IList<FilterSpecParam>[] { BuildList(eventType, objects) }, null);
        }
    
        public static List<FilterSpecParam> BuildList(EventType eventType, Object[] objects)
        {
            var filterParams = new List<FilterSpecParam>();
    
            var index = 0;
            while (objects.Length > index)
            {
                var propertyName = (String) objects[index++];
                var filterOperator = (FilterOperator) objects[index++];
    
                if (!(filterOperator.IsRangeOperator()))
                {
                    var filterForConstant = objects[index++];
                    filterParams.Add(new FilterSpecParamConstant(MakeLookupable(eventType, propertyName), filterOperator, filterForConstant));
                }
                else
                {
                    var min = objects[index++].AsDouble();
                    var max = objects[index++].AsDouble();
                    filterParams.Add(new FilterSpecParamRange(MakeLookupable(eventType, propertyName), filterOperator,
                            new FilterForEvalConstantDouble(min),
                            new FilterForEvalConstantDouble(max)));
                }
            }
    
            return filterParams;
        }
    
        private static FilterSpecLookupable MakeLookupable(EventType eventType, String fieldName) {
            return new FilterSpecLookupable(fieldName, eventType.GetGetter(fieldName), eventType.GetPropertyType(fieldName), false);
        }
    }
    
    
}
