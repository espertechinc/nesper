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
    public interface MultiKeyFromMultiKey
    {
        object From(object multiKey);
    }

    public class ProxyMultiKeyFromMultiKey : MultiKeyFromMultiKey
    {
        public Func<object, object> ProcFrom { get; set; }

        public ProxyMultiKeyFromMultiKey()
        {
        }

        public ProxyMultiKeyFromMultiKey(Func<object, object> procFrom)
        {
            ProcFrom = procFrom;
        }

        public object From(object multiKey)
        {
            return ProcFrom.Invoke(multiKey);
        }
    }
} // end of namespace