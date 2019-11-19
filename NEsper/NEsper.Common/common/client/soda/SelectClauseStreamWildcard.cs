///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// For use in a select clause, this element in a select clause defines that for a
    /// given stream we want to select the underlying type. Most often used in joins to
    /// select wildcard from one of the joined streams.
    /// <para/>
    /// For example: <pre>select streamOne.* from StreamOne as streamOne, StreamTwo as
    /// streamTwo</pre>
    /// </summary>
    [Serializable]
    public class SelectClauseStreamWildcard : SelectClauseElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectClauseStreamWildcard"/> class.
        /// </summary>
        public SelectClauseStreamWildcard()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamName">is the name assigned to a stream</param>
        /// <param name="optionalColumnName">is the name to assign to the column carrying the streams generated events, ornull if the event should not appear in a column </param>
        public SelectClauseStreamWildcard(
            string streamName,
            string optionalColumnName)
        {
            this.StreamName = streamName;
            this.OptionalColumnName = optionalColumnName;
        }

        /// <summary>
        /// Returns the stream name (e.g. select streamName.* as colName from MyStream as
        /// streamName)
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        public string StreamName { get; set; }

        /// <summary>
        /// Returns the optional column name (e.g. select streamName.* as colName from
        /// MyStream as streamName)
        /// </summary>
        /// <returns>
        /// name of column, or null if none defined
        /// </returns>
        public string OptionalColumnName { get; set; }

        /// <summary>
        /// Renders the element in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPLElement(TextWriter writer)
        {
            writer.Write(StreamName);
            writer.Write(".*");
            if (OptionalColumnName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalColumnName);
            }
        }
    }
}