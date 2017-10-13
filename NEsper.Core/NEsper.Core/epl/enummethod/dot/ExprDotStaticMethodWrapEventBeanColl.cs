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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapEventBeanColl : ExprDotStaticMethodWrap
    {
        private readonly EventType _type;

        public ExprDotStaticMethodWrapEventBeanColl(EventType type)
        {
            _type = type;
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.CollectionOfEvents(_type); }
        }

        public ICollection<object> Convert(Object result)
        {
            return result.Unwrap<object>();
        }
    }
} // end of namespace
