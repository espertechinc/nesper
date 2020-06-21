///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class MultiKeyPlan
    {
        public MultiKeyPlan(
            IList<StmtClassForgeableFactory> multiKeyForgeables,
            MultiKeyClassRef classRef)
        {
            MultiKeyForgeables = multiKeyForgeables;
            ClassRef = classRef;
        }

        public IList<StmtClassForgeableFactory> MultiKeyForgeables { get; }

        public MultiKeyClassRef ClassRef { get; }
    }
} // end of namespace