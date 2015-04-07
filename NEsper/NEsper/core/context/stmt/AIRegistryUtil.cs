///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryUtil
    {
        public static void CheckExpand<T>(int serviceId, ArrayWrap<T> services)
        {
            if (serviceId > services.Array.Length - 1)
            {
                int delta = serviceId - services.Array.Length + 1;
                services.Expand(delta);
            }
        }
    }
}