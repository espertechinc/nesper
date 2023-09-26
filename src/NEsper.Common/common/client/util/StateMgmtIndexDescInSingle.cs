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
    public class StateMgmtIndexDescInSingle
    {
        private readonly string indexedProp;
        private readonly MultiKeyClassRef multiKeyPlan;

        public StateMgmtIndexDescInSingle(
            string indexedProp,
            MultiKeyClassRef multiKeyPlan)
        {
            this.indexedProp = indexedProp;
            this.multiKeyPlan = multiKeyPlan;
        }

        public string IndexedProp => indexedProp;

        public MultiKeyClassRef MultiKeyPlan => multiKeyPlan;
    }
} // end of namespace