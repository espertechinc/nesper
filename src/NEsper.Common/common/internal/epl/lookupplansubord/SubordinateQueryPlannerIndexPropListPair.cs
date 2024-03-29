///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.join.lookup;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlannerIndexPropListPair
    {
        public SubordinateQueryPlannerIndexPropListPair(
            IList<IndexedPropDesc> hashedProps,
            IList<IndexedPropDesc> btreeProps)
        {
            HashedProps = hashedProps;
            BtreeProps = btreeProps;
        }

        public IList<IndexedPropDesc> HashedProps { get; }

        public IList<IndexedPropDesc> BtreeProps { get; }
    }
} // end of namespace