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

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Reader for fast access to all event properties for an event backed by a Java object.
    /// </summary>
    public class BeanEventBeanReader : EventBeanReader
    {
        private BeanEventPropertyGetter[] _getterArray;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">the type of read</param>
        public BeanEventBeanReader(BeanEventType type)
        {
            var properties = type.PropertyNames;
            var getters = new List<BeanEventPropertyGetter>();
            foreach (string property in properties)
            {
                var getter = (BeanEventPropertyGetter)type.GetGetterSPI(property);
                if (getter != null)
                {
                    getters.Add(getter);
                }
            }
            _getterArray = getters.ToArray();
        }

        public Object[] Read(EventBean theEvent)
        {
            var underlying = theEvent.Underlying;
            var values = new Object[_getterArray.Length];
            for (int i = 0; i < _getterArray.Length; i++)
            {
                values[i] = _getterArray[i].GetBeanProp(underlying);
            }
            return values;
        }
    }
} // end of namespace
