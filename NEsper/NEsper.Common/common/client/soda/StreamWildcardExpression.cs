///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents "stream.*" in for example "mystream.*"
    /// </summary>
    [Serializable]
    public class StreamWildcardExpression : ExpressionBase {

	    private string streamName;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamName">stream name</param>
	    public StreamWildcardExpression(string streamName) {
	        this.streamName = streamName;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public StreamWildcardExpression() {
	    }

	    /// <summary>
	    /// Returns the stream name.
	    /// </summary>
	    /// <returns>stream name</returns>
	    public string StreamName
	    {
	        get => streamName;
	    }

	    /// <summary>
	    /// Sets the stream name.
	    /// </summary>
	    /// <param name="streamName">stream name</param>
	    public void SetStreamName(string streamName) {
	        this.streamName = streamName;
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer) {
	        writer.Write(streamName);
	        writer.Write(".*");
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get => ExpressionPrecedenceEnum.UNARY;
	    }
	}
} // end of namespace