///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>Base getter for native fragments. </summary>
    public abstract class BaseNativePropertyGetter : EventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private volatile BeanEventType _fragmentEventType;
        private readonly Type _fragmentClassType;
        private bool _isFragmentable;
        private readonly bool _isArray;
        private readonly bool _isIterable;

        public abstract object Get(EventBean eventBean);
        public abstract bool IsExistsProperty(EventBean eventBean);

        /// <summary>Constructor. </summary>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="genericType">type generic parameter, if any</param>
        protected BaseNativePropertyGetter(EventAdapterService eventAdapterService, Type returnType, Type genericType)
        {
            _eventAdapterService = eventAdapterService;
            if (returnType.IsArray)
            {
                _fragmentClassType = returnType.GetElementType();
                _isArray = true;
                _isIterable = false;
            }
            else if (returnType.IsImplementsInterface(typeof(IEnumerable)))
            {
                _fragmentClassType = genericType;
                _isArray = false;
                _isIterable = true;
            }
            else
            {
                _fragmentClassType = returnType;
                _isArray = false;
                _isIterable = false;
            }
            _isFragmentable = true;
        }

        /// <summary>Returns the fragment for dynamic properties. </summary>
        /// <param name="object">to inspect</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <returns>fragment</returns>
        public static Object GetFragmentDynamic(Object @object, EventAdapterService eventAdapterService)
        {
            if (@object == null)
            {
                return null;
            }

            BeanEventType fragmentEventType = null;
            bool isArray = false;
            if (@object.GetType().IsArray)
            {
                if (@object.GetType().GetElementType().IsFragmentableType())
                {
                    isArray = true;
                    fragmentEventType = eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(@object.GetType().GetElementType());
                }
            }
            else
            {
                if (@object.GetType().IsFragmentableType())
                {
                    fragmentEventType = eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(@object.GetType());
                }
            }

            if (fragmentEventType == null)
            {
                return null;
            }

            if (isArray)
            {
                var asArray = (Array) @object;
                var len = asArray.Length;
                var events = new EventBean[len];
                int countFilled = 0;

                for (int i = 0; i < len; i++)
                {
                    Object element = asArray.GetValue(i);
                    if (element == null)
                    {
                        continue;
                    }

                    events[countFilled] = eventAdapterService.AdapterForTypedObject(element, fragmentEventType);
                    countFilled++;
                }

                if (countFilled == len)
                {
                    return events;
                }

                if (countFilled == 0)
                {
                    return new EventBean[0];
                }

                var returnVal = new EventBean[countFilled];
                Array.Copy(events, 0, returnVal, 0, countFilled);
                return returnVal;
            }
            else
            {
                return eventAdapterService.AdapterForTypedObject(@object, fragmentEventType);
            }
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object @object = Get(eventBean);
            if (@object == null)
            {
                return null;
            }

            if (!_isFragmentable)
            {
                return null;
            }

            if (_fragmentEventType == null)
            {
                if (_fragmentClassType.IsFragmentableType())
                {
                    _fragmentEventType = _eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(_fragmentClassType);
                }
                else
                {
                    _isFragmentable = false;
                    return null;
                }
            }

            if (_isArray)
            {
                var asArray = (Array) @object;
                var len = asArray.Length;
                var events = new EventBean[len];
                int countFilled = 0;

                for (int i = 0; i < len; i++)
                {
                    Object element = asArray.GetValue(i);
                    if (element == null)
                    {
                        continue;
                    }

                    events[countFilled] = _eventAdapterService.AdapterForTypedObject(element, _fragmentEventType);
                    countFilled++;
                }

                if (countFilled == len)
                {
                    return events;
                }

                if (countFilled == 0)
                {
                    return new EventBean[0];
                }

                var returnVal = new EventBean[countFilled];
                Array.Copy(events, 0, returnVal, 0, countFilled);
                return returnVal;
            }
            else if (_isIterable)
            {
                if (!(@object is IEnumerable))
                {
                    return null;
                }
                IEnumerator enumerator = ((IEnumerable)@object).GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return new EventBean[0];
                }
                var events = new List<EventBean>();
                do
                {
                    Object next = enumerator.Current;
                    if (next == null)
                    {
                        continue;
                    }

                    events.Add(_eventAdapterService.AdapterForTypedObject(next, _fragmentEventType));
                } while (enumerator.MoveNext());

                return events.ToArray();
            }
            else
            {
                return _eventAdapterService.AdapterForTypedObject(@object, _fragmentEventType);
            }
        }
    }
}