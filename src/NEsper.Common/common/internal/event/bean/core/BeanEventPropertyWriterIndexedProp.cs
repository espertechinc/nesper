///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    public class BeanEventPropertyWriterIndexedProp : BeanEventPropertyWriter
    {
        private readonly int _index;

        public BeanEventPropertyWriterIndexedProp(
            Type clazz,
            MethodInfo writerMember,
            int index)
            : base(clazz, writerMember)
        {
            _index = index;
        }

        public override void Write(
            object value,
            EventBean target)
        {
            Invoke(new[] { _index, value }, target.Underlying);
        }
    }
} // end of namespace