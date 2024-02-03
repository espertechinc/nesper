///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.client.util
{
    public class StateMgmtIndexDescComposite
    {
        public StateMgmtIndexDescComposite(
            string[] indexedProps,
            MultiKeyClassRef multiKeyPlan,
            string[] indexedRangeProps,
            DataInputOutputSerdeForge[] rangeSerdes)
        {
            IndexedProps = indexedProps;
            MultiKeyPlan = multiKeyPlan;
            IndexedRangeProps = indexedRangeProps;
            RangeSerdes = rangeSerdes;
        }

        public string[] IndexedProps { get; }

        public MultiKeyClassRef MultiKeyPlan { get; }

        public string[] IndexedRangeProps { get; }

        public DataInputOutputSerdeForge[] RangeSerdes { get; }
    }
} // end of namespace