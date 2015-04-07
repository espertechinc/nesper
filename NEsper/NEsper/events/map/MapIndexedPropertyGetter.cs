///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.map
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Getter for a dynamic indexed property for maps.
    /// </summary>
    public class MapIndexedPropertyGetter : MapEventPropertyGetter
    {
        private readonly int _index;
        private readonly String _fieldName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="index">index to get the element at</param>
        public MapIndexedPropertyGetter(String fieldName, int index)
        {
            _index = index;
            _fieldName = fieldName;
        }

        public Object GetMap(DataMap map)
        {
            Object value = map.Get(_fieldName);
            return BaseNestableEventUtil.GetIndexedValue(value, _index);
        }

        public bool IsMapExistsProperty(DataMap map)
        {
            Object value = map.Get(_fieldName);
            return BaseNestableEventUtil.IsExistsIndexedValue(value, _index);
        }

        public Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
