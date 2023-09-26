///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.multikey;

namespace com.espertech.esper.common.client.util
{
    public class StateMgmtIndexDescHash
    {
        public StateMgmtIndexDescHash(
            string[] indexedProps,
            MultiKeyClassRef multiKeyPlan,
            bool unique)
        {
            IndexedProps = indexedProps;
            MultiKeyPlan = multiKeyPlan;
            IsUnique = unique;
        }

        public string[] IndexedProps { get; }

        public MultiKeyClassRef MultiKeyPlan { get; }

        public bool IsUnique { get; }
    }
} // end of namespace