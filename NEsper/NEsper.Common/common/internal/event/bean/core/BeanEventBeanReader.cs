///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Reader for fast access to all event properties for an event backed by an object.
    /// </summary>
    public class BeanEventBeanReader : EventBeanReader
    {
        private readonly BeanEventPropertyGetter[] getterArray;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">the type of read</param>
        public BeanEventBeanReader(BeanEventType type)
        {
            var properties = type.PropertyNames;
            var getters = new List<BeanEventPropertyGetter>();
            foreach (var property in properties) {
                var getter = (BeanEventPropertyGetter) type.GetGetterSPI(property);
                if (getter != null) {
                    getters.Add(getter);
                }
            }

            getterArray = getters.ToArray();
        }

        public object[] Read(EventBean theEvent)
        {
            var underlying = theEvent.Underlying;
            var values = new object[getterArray.Length];
            for (var i = 0; i < getterArray.Length; i++) {
                values[i] = getterArray[i].GetBeanProp(underlying);
            }

            return values;
        }
    }
} // end of namespace