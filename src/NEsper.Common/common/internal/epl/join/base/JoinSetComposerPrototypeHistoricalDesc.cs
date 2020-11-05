///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeHistoricalDesc
    {
        public JoinSetComposerPrototypeHistoricalDesc(
            HistoricalIndexLookupStrategyForge lookupForge,
            PollResultIndexingStrategyForge indexingForge,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            LookupForge = lookupForge;
            IndexingForge = indexingForge;
            AdditionalForgeables = additionalForgeables;
        }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public PollResultIndexingStrategyForge IndexingForge { get; }

        public HistoricalIndexLookupStrategyForge LookupForge { get; }
    }
} // end of namespace