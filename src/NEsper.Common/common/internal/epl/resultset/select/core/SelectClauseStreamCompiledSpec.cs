///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectClauseStreamCompiledSpec : SelectClauseElementCompiled
    {
        private bool isFragmentEvent;
        private int streamNumber = -1;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamName">is the stream name of the stream to select</param>
        /// <param name="optionalColumnName">is the column name</param>
        public SelectClauseStreamCompiledSpec(
            string streamName,
            string optionalColumnName)
        {
            StreamName = streamName;
            OptionalName = optionalColumnName;
        }

        /// <summary>
        ///     Returns the stream name (e.g. select streamName from MyEvent as streamName).
        /// </summary>
        /// <returns>name</returns>
        public string StreamName { get; }

        /// <summary>
        ///     Returns the column name.
        /// </summary>
        /// <returns>name</returns>
        public string OptionalName { get; }

        /// <summary>
        ///     Returns true to indicate that we are meaning to select a tagged event in a pattern, or false if
        ///     selecting an event from a stream.
        /// </summary>
        /// <value>true for tagged event in pattern, false for stream</value>
        public bool IsFragmentEvent {
            get {
                if (streamNumber == -1) {
                    throw new IllegalStateException("Not initialized for stream number and tagged event");
                }

                return isFragmentEvent;
            }
            set => isFragmentEvent = value;
        }

        /// <summary>
        ///     Sets the stream number of the selected stream within the context of the from-clause.
        /// </summary>
        /// <value>to set</value>
        public int StreamNumber {
            set => streamNumber = value;
            get {
                if (streamNumber == -1) {
                    throw new IllegalStateException("Not initialized for stream number and tagged event");
                }

                return streamNumber;
            }
        }

        /// <summary>
        ///     True if selecting from a property, false if not
        /// </summary>
        /// <returns>indicator whether property or not</returns>
        public bool IsProperty { get; private set; }

        /// <summary>
        ///     Returns property type.
        /// </summary>
        /// <returns>property type</returns>
        public Type PropertyType { get; private set; }

        public TableMetaData TableMetadata { get; set; }

        /// <summary>
        ///     Sets an indicate that a property was selected with wildcard.
        /// </summary>
        /// <param name="property">selected</param>
        /// <param name="propertyType">the return type</param>
        public void SetProperty(
            bool property,
            Type propertyType)
        {
            IsProperty = property;
            PropertyType = propertyType;
        }
    }
} // end of namespace