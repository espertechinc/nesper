///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    /// <summary>
    ///     Cache entry bean-to-collection-of-bean.
    /// </summary>
    public class ExpressionResultCacheEntryBeanAndCollBean
    {
        public ExpressionResultCacheEntryBeanAndCollBean(
            EventBean reference,
            ICollection<EventBean> result)
        {
            Reference = reference;
            Result = result;
        }

        public EventBean Reference { get; set; }

        public ICollection<EventBean> Result { get; set; }
    }
} // end of namespace