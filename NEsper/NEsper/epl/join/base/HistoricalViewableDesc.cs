///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.@base
{
    public class HistoricalViewableDesc
    {
        public HistoricalViewableDesc(int numStreams)
        {
            DependenciesPerHistorical = new SortedSet<int>[numStreams];
            Historical = new bool[numStreams];
        }

        public bool HasHistorical { get; private set; }

        public ICollection<int>[] DependenciesPerHistorical { get; private set; }

        public bool[] Historical { get; private set; }

        public void SetHistorical(int streamNum, ICollection<int> dependencies)
        {
            HasHistorical = true;
            Historical[streamNum] = true;
            if (DependenciesPerHistorical[streamNum] != null)
            {
                throw new EPException("Dependencies for stream " + streamNum + "already initialized");
            }
            DependenciesPerHistorical[streamNum] = new SortedSet<int>();
            if (dependencies != null)
            {
                DependenciesPerHistorical[streamNum].AddAll(dependencies);
            }
        }
    }
}