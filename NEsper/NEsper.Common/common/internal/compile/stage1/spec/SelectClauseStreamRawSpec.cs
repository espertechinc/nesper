///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// For use in select clauses for specifying a selected stream:
    ///     select a.* from MyEvent as a, MyOther as b
    /// </summary>
    public class SelectClauseStreamRawSpec : SelectClauseElementRaw
    {
        private readonly string streamName;
        private readonly string optionalAsName;

        /// <summary>Ctor. </summary>
        /// <param name="streamName">is the stream name of the stream to select</param>
        /// <param name="optionalAsName">is the column name</param>
        public SelectClauseStreamRawSpec(
            string streamName,
            string optionalAsName)
        {
            this.streamName = streamName;
            this.optionalAsName = optionalAsName;
        }

        /// <summary>
        /// Returns the stream name (e.g. select streamName from MyEvent as streamName).
        /// </summary>
        /// <value>The name of the stream.</value>
        public string StreamName {
            get { return streamName; }
        }

        /// <summary>
        /// Returns the column alias (e.g. select streamName as mycol from MyEvent as streamName).
        /// </summary>
        /// <value>The name of the optional as.</value>
        public string OptionalAsName {
            get { return optionalAsName; }
        }
    }
}