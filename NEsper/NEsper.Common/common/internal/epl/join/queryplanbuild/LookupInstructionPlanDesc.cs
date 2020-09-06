///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.join.queryplanouter;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    public class LookupInstructionPlanDesc
    {
        public LookupInstructionPlanDesc(
            IList<LookupInstructionPlanForge> forges,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            Forges = forges;
            AdditionalForgeables = additionalForgeables;
        }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public IList<LookupInstructionPlanForge> Forges { get; }
    }
} // end of namespace