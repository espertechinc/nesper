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
    /// Getter for one or more levels deep nested properties.
    /// </summary>
    public sealed class NestedPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter[] _getterChain;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdapterService">is the cache and factory for event bean types and event wrappers</param>
        /// <param name="finalPropertyType">type of the entry returned</param>
        /// <param name="finalGenericType">generic type parameter of the entry returned, if any</param>
        public NestedPropertyGetter(IList<EventPropertyGetter> getterChain, EventAdapterService eventAdapterService, Type finalPropertyType, Type finalGenericType)
            : base(eventAdapterService, finalPropertyType, finalGenericType)
        {
            _getterChain = new BeanEventPropertyGetter[getterChain.Count];

            unchecked
            {
                for (int ii = 0; ii < getterChain.Count; ii++)
                {
                    _getterChain[ii] = (BeanEventPropertyGetter) getterChain[ii];
                }
            }
        }

        public Object GetBeanProp(Object value)
        {
            if (value == null)
            {
                return value;
            }

            unchecked
            {
                var getterChain = _getterChain;
                for (int ii = 0; ii < getterChain.Length; ii++)
                {
                    value = getterChain[ii].GetBeanProp(value);

                    if (value == null)
                    {
                        return null;
                    }
                }
            }
            return value;
        }

        public bool IsBeanExistsProperty(Object value)
        {
            if (value == null)
            {
                return false;
            }

            int lastElementIndex = _getterChain.Length - 1;

            // walk the getter chain up to the previous-to-last element, returning its object value.
            // any null values in between mean the property does not exists
            for (int i = 0; i < _getterChain.Length - 1; i++)
            {
                value = _getterChain[i].GetBeanProp(value);

                if (value == null)
                {
                    return false;
                }
            }

            return _getterChain[lastElementIndex].IsBeanExistsProperty(value);
        }

        public override Object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return IsBeanExistsProperty(eventBean.Underlying);
        }
    }
}
