///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
	/// <summary>
	/// Assembly factory node for an event stream that is a root with a one optional child node below it.
	/// </summary>
	public class RootOptionalAssemblyNodeFactory : BaseAssemblyNodeFactory
	{
	    public RootOptionalAssemblyNodeFactory(int streamNum, int numStreams)
            : base(streamNum, numStreams)
	    {
	    }

	    public override void Print(IndentWriter indentWriter)
	    {
	        indentWriter.WriteLine("RootOptionalAssemblyNode streamNum=" + StreamNum);
	    }

	    public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
	        return new RootOptionalAssemblyNode(StreamNum, NumStreams);
	    }
	}
} // end of namespace
