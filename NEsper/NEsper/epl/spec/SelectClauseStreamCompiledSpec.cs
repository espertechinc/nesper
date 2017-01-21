///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Mirror class to <seealso cref="SelectClauseStreamRawSpec"/> but added the stream
    /// number for the name.
    /// </summary>
    public class SelectClauseStreamCompiledSpec : SelectClauseElementCompiled
    {
        private int streamNumber = -1;
        private bool isFragmentEvent = false;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamName">is the stream name of the stream to select</param>
        /// <param name="optionalColumnName">is the column name</param>
        public SelectClauseStreamCompiledSpec(String streamName, String optionalColumnName)
        {
            IsProperty = false;
            StreamName = streamName;
            OptionalName = optionalColumnName;
        }

        /// <summary>
        /// Returns the stream name (e.g. select streamName from MyEvent as streamName).
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        public string StreamName { get; private set; }

        /// <summary>
        /// Returns the column name.
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        public string OptionalName { get; private set; }

        /// <summary>
        /// Returns the stream number of the stream for the stream name.
        /// </summary>
        /// <returns>
        /// stream number
        /// </returns>
        public int StreamNumber
        {
            get
            {
                if (streamNumber == -1) {
                    throw new IllegalStateException("Not initialized for stream number and tagged event");
                }
                return streamNumber;
            }
            set { streamNumber = value; }
        }

        /// <summary>
        /// Returns true to indicate that we are meaning to select a tagged event in a
        /// pattern, or false if selecting an event from a stream.
        /// </summary>
        /// <returns>
        /// true for tagged event in pattern, false for stream
        /// </returns>
        public bool IsFragmentEvent
        {
            get
            {
                if (streamNumber == -1) {
                    throw new IllegalStateException("Not initialized for stream number and tagged event");
                }
                return isFragmentEvent;
            }
            set { isFragmentEvent = value; }
        }

        /// <summary>
        /// Sets an indicate that a property was selected with wildcard.
        /// </summary>
        /// <param name="property">selected</param>
        /// <param name="propertyType">the return type</param>
        public void SetProperty(bool property, Type propertyType)
        {
            this.IsProperty = property;
            this.PropertyType = propertyType;
        }

        /// <summary>
        /// True if selecting from a property, false if not
        /// </summary>
        /// <returns>
        /// indicator whether property or not
        /// </returns>
        public bool IsProperty { get; set; }

        /// <summary>
        /// Returns property type.
        /// </summary>
        /// <returns>
        /// property type
        /// </returns>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the table metadata.
        /// </summary>
        /// <value>
        /// The table metadata.
        /// </value>
        public TableMetadata TableMetadata { get; set; }
    }
}
