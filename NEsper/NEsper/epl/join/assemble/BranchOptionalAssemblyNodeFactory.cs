///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
	/// <summary>
	/// Assembly factory node for an event stream that is a branch with a single optional child node below it.
	/// </summary>
	public class BranchOptionalAssemblyNodeFactory : BaseAssemblyNodeFactory
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">is the stream number</param>
	    /// <param name="numStreams">is the number of streams</param>
	    public BranchOptionalAssemblyNodeFactory(int streamNum, int numStreams)
	        : base(streamNum, numStreams)
	    {
	    }

	    public override void Print(IndentWriter indentWriter)
	    {
	        indentWriter.WriteLine("BranchOptionalAssemblyNode streamNum=" + StreamNum);
	    }

	    public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
	        return new BranchOptionalAssemblyNode(StreamNum, NumStreams);
	    }
	}
} // end of namespace
