///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.epl.enummethod.dot
{
    [Serializable]
    public class ExprDotStaticMethodWrapIterableEvents : ExprDotStaticMethodWrap
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly BeanEventType _type;

        public ExprDotStaticMethodWrapIterableEvents(EventAdapterService eventAdapterService, BeanEventType type)
        {
            _eventAdapterService = eventAdapterService;
            _type = type;
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.CollectionOfEvents(_type); }
        }

        public ICollection<object> Convert(Object result)
        {
            if (result == null)
            {
                return null;
            }

            var asEnum = result.Unwrap<object>();
            if (asEnum == null)
            {
                return null;
            }

            return asEnum
                .Select(item => _eventAdapterService.AdapterForTypedObject(item, _type))
                .ToArray();
        }
    }
}
