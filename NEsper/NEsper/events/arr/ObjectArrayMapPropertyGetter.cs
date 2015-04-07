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
using com.espertech.esper.events.map;

namespace com.espertech.esper.events.arr
{
    using Map = IDictionary<string, object>;

    public class ObjectArrayMapPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _index;
        private readonly MapEventPropertyGetter _getter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        public ObjectArrayMapPropertyGetter(int index, MapEventPropertyGetter getter)
        {
            if (getter == null)
            {
                throw new ArgumentException("Getter is a required parameter");
            }
            _index = index;
            _getter = getter;
        }

        public Object GetObjectArray(Object[] array)
        {
            Object valueTopObj = array[_index];
            if (!(valueTopObj is Map))
            {
                return null;
            }
            return _getter.GetMap((Map)valueTopObj);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            Object valueTopObj = array[_index];
            if (!(valueTopObj is Map))
            {
                return false;
            }

            return _getter.IsMapExistsProperty((Map)valueTopObj);
        }

        public Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsObjectArrayExistsProperty(array);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
