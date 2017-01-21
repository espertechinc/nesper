///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class BeanEventPropertyWriterIndexedProp : BeanEventPropertyWriter {
        private readonly int _index;

        public BeanEventPropertyWriterIndexedProp(Type clazz, FastMethod writerMethod, int index)
            : base(clazz, writerMethod)
        {
            _index = index;
        }
    
        public override void Write(Object value, EventBean target) 
        {
            base.Invoke(new Object[] {_index, value}, target.Underlying);
        }
    }
}
