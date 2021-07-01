///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    public class HistoricalViewableDesc
    {
        public HistoricalViewableDesc(int numStreams)
        {
            DependenciesPerHistorical = new SortedSet<int>[numStreams];
            Historical = new bool[numStreams];
        }

        public bool IsHistorical { get; private set; }

        public SortedSet<int>[] DependenciesPerHistorical { get; }

        public bool[] Historical { get; }

        public void SetHistorical(
            int streamNum,
            SortedSet<int> dependencies)
        {
            IsHistorical = true;
            Historical[streamNum] = true;
            if (DependenciesPerHistorical[streamNum] != null) {
                throw new EPRuntimeException("Dependencies for stream " + streamNum + "already initialized");
            }

            DependenciesPerHistorical[streamNum] = new SortedSet<int>();
            if (dependencies != null) {
                DependenciesPerHistorical[streamNum].AddAll(dependencies);
            }
        }
    }
} // end of namespace