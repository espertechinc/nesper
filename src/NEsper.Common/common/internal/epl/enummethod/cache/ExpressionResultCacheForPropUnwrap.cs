///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    /// <summary>
    /// On the level of indexed event properties: Properties that are contained in EventBean instances, such as for Enumeration Methods,
    /// get wrapped only once for the same event.  The cache is keyed by property-name and EventBean reference and maintains a FlexCollection.
    /// <para>
    /// NOTE: ExpressionResultCacheForPropUnwrap should not be held onto since the instance returned can be reused.
    /// </para>
    /// </summary>
    public interface ExpressionResultCacheForPropUnwrap
    {
        ExpressionResultCacheEntryBeanAndCollBean GetPropertyColl(
            string propertyNameFullyQualified,
            EventBean reference);

        void SavePropertyColl(
            string propertyNameFullyQualified,
            EventBean reference,
            ICollection<EventBean> events);
    }
} // end of namespace