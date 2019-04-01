///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class IndexKeyInfo
    {
        public IndexKeyInfo(
            IList<SubordPropHashKeyForge> orderedKeyProperties, CoercionDesc orderedKeyCoercionTypes,
            IList<SubordPropRangeKeyForge> orderedRangeDesc, CoercionDesc orderedRangeCoercionTypes)
        {
            OrderedHashDesc = orderedKeyProperties;
            OrderedKeyCoercionTypes = orderedKeyCoercionTypes;
            OrderedRangeDesc = orderedRangeDesc;
            OrderedRangeCoercionTypes = orderedRangeCoercionTypes;
        }

        public IList<SubordPropHashKeyForge> OrderedHashDesc { get; }

        public CoercionDesc OrderedKeyCoercionTypes { get; }

        public IList<SubordPropRangeKeyForge> OrderedRangeDesc { get; }

        public CoercionDesc OrderedRangeCoercionTypes { get; }
    }
} // end of namespace