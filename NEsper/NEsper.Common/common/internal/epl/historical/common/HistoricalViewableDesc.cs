///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
	public class HistoricalViewableDesc {
	    private bool hasHistorical;
	    private readonly SortedSet<int>[] dependenciesPerHistorical;
	    private readonly bool[] isHistorical;

	    public HistoricalViewableDesc(int numStreams) {
	        this.dependenciesPerHistorical = new SortedSet[numStreams];
	        this.isHistorical = new bool[numStreams];
	    }

	    public void SetHistorical(int streamNum, SortedSet<int> dependencies) {
	        hasHistorical = true;
	        isHistorical[streamNum] = true;
	        if (dependenciesPerHistorical[streamNum] != null) {
	            throw new RuntimeException("Dependencies for stream " + streamNum + "already initialized");
	        }
	        dependenciesPerHistorical[streamNum] = new SortedSet<int>();
	        if (dependencies != null) {
	            dependenciesPerHistorical[streamNum].AddAll(dependencies);
	        }
	    }

	    public bool IsHistorical
	    {
	        get => hasHistorical;
	    }

	    public SortedSet<int>[] DependenciesPerHistorical
	    {
	        get => dependenciesPerHistorical;
	    }

	    public bool[] Historical
	    {
	        get => isHistorical;
	    }
	}
} // end of namespace