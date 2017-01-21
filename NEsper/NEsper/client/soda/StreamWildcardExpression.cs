///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents "stream.*" in for example "mystream.*"
    /// </summary>
    [Serializable]
    public class StreamWildcardExpression : ExpressionBase
    {
        private string _streamName;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamName">stream name</param>
        public StreamWildcardExpression(string streamName)
        {
            this._streamName = streamName;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public StreamWildcardExpression()
        {
        }

        /// <summary>
        /// Returns the stream name.
        /// </summary>
        /// <value>stream name</value>
        public string StreamName
        {
            get { return _streamName; }
            set { this._streamName = value; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_streamName);
            writer.Write(".*");
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }
    }
}
