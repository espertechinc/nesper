///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using XLR8.CGLib;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    public class BeanEventPropertyWriterMapProp : BeanEventPropertyWriter
    {
        private readonly String _key;

        public BeanEventPropertyWriterMapProp(Type clazz, FastMethod writerMethod, String key)
            : base(clazz, writerMethod)
        {
            _key = key;
        }
    
        public override void Write(Object value, EventBean target) 
        {
            Invoke(new Object[] {_key, value}, target.Underlying);
        }
    }
}
