///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class IndexKeyInfo
    {
        public IndexKeyInfo(IList<SubordPropHashKey> orderedKeyDesc, CoercionDesc orderedKeyCoercionTypes, IList<SubordPropRangeKey> orderedRangeDesc, CoercionDesc orderedRangeCoercionTypes)
        {
            OrderedHashDesc = orderedKeyDesc;
            OrderedKeyCoercionTypes = orderedKeyCoercionTypes;
            OrderedRangeDesc = orderedRangeDesc;
            OrderedRangeCoercionTypes = orderedRangeCoercionTypes;
        }

        public IList<SubordPropHashKey> OrderedHashDesc { get; private set; }

        public CoercionDesc OrderedKeyCoercionTypes { get; private set; }

        public IList<SubordPropRangeKey> OrderedRangeDesc { get; private set; }

        public CoercionDesc OrderedRangeCoercionTypes { get; private set; }
    }
}
