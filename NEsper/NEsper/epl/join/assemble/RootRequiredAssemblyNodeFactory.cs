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
	/// Assembly node factory for an event stream that is a root with a one required child node below it.
	/// </summary>
	public class RootRequiredAssemblyNodeFactory : BaseAssemblyNodeFactory
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">is the stream number</param>
	    /// <param name="numStreams">is the number of streams</param>
	    public RootRequiredAssemblyNodeFactory(int streamNum, int numStreams)
	        : base(streamNum, numStreams)
	    {
	    }

	    public override void Print(IndentWriter indentWriter)
	    {
	        indentWriter.WriteLine("RootRequiredAssemblyNode streamNum=" + StreamNum);
	    }

	    public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
	        return new RootRequiredAssemblyNode(StreamNum, NumStreams);
	    }
	}
} // end of namespace
