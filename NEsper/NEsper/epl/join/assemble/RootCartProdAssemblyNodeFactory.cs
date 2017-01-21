///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
	/// <summary>
	/// Assembly factory node for an event stream that is a root with a two or more child nodes below it.
	/// </summary>
	public class RootCartProdAssemblyNodeFactory : BaseAssemblyNodeFactory
	{
	    private readonly int[] _childStreamIndex; // maintain mapping of stream number to index in array
	    private readonly bool _allSubStreamsOptional;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">is the stream number</param>
	    /// <param name="numStreams">is the number of streams</param>
	    /// <param name="allSubStreamsOptional">true if all substreams are optional and none are required</param>
	    public RootCartProdAssemblyNodeFactory(int streamNum, int numStreams, bool allSubStreamsOptional)
            : base(streamNum, numStreams)
	    {
	        _allSubStreamsOptional = allSubStreamsOptional;
	        _childStreamIndex = new int[numStreams];
	    }

	    public override void AddChild(BaseAssemblyNodeFactory childNode)
	    {
	        _childStreamIndex[childNode.StreamNum] = ChildNodes.Count;
	        base.AddChild(childNode);
	    }

	    public override void Print(IndentWriter indentWriter)
	    {
	        indentWriter.WriteLine("RootCartProdAssemblyNode streamNum=" + StreamNum);
	    }

	    public override BaseAssemblyNode MakeAssemblerUnassociated() 
        {
	        return new RootCartProdAssemblyNode(StreamNum, NumStreams, _allSubStreamsOptional, _childStreamIndex);
	    }
	}
} // end of namespace
