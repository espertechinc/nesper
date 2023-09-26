///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.collection
{
    public interface MultiKeyFromObjectArray
    {
        object From(object[] values);
    }

    public class ProxyMultiKeyFromObjectArray : MultiKeyFromObjectArray
    {
        public Func<object[], object> ProcFrom { get; set; }

        public ProxyMultiKeyFromObjectArray()
        {
        }

        public ProxyMultiKeyFromObjectArray(Func<object[], object> procFrom)
        {
            ProcFrom = procFrom;
        }

        public object From(object[] values)
        {
            return ProcFrom.Invoke(values);
        }
    }
} // end of namespace