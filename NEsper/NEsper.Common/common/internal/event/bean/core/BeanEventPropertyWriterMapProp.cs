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
    public class BeanEventPropertyWriterMapProp : BeanEventPropertyWriter
    {
        private readonly string key;

        public BeanEventPropertyWriterMapProp(
            Type clazz,
            MethodInfo writerMethod,
            string key)
            : base(clazz, writerMethod)
        {
            this.key = key;
        }

        public void Write(
            object value,
            EventBean target)
        {
            Invoke(new[] {key, value}, target.Underlying);
        }
    }
} // end of namespace