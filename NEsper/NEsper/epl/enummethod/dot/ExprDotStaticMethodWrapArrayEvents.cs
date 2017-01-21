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

using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapArrayEvents : ExprDotStaticMethodWrap
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly BeanEventType _type;

        public ExprDotStaticMethodWrapArrayEvents(EventAdapterService eventAdapterService, BeanEventType type)
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

            var asArray = result as Array;
            if (asArray == null)
            {
                return null;
            }

            return asArray.Cast<object>()
                .Select(item => _eventAdapterService.AdapterForTypedObject(item, _type))
                .Cast<object>()
                .ToList();
        }
    }
}
