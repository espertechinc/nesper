///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For Avro types for widening objects to Avro record values, see <seealso cref="ObjectValueTypeWidenerFactory" />
    /// </summary>
    public class ObjectValueTypeWidenerFactoryContext
    {
        private readonly Type clazz;
        private readonly string propertyName;
        private readonly EventType eventType;
        private readonly string statementName;
        private readonly string engineURI;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="eventType">event type</param>
        /// <param name="statementName">statement name</param>
        /// <param name="engineURI">engine URI</param>
        public ObjectValueTypeWidenerFactoryContext(
            Type clazz,
            string propertyName,
            EventType eventType,
            string statementName,
            string engineURI)
        {
            this.clazz = clazz;
            this.propertyName = propertyName;
            this.eventType = eventType;
            this.statementName = statementName;
            this.engineURI = engineURI;
        }

        /// <summary>
        /// Returns the class
        /// </summary>
        /// <returns>class</returns>
        public Type Clazz {
            get => clazz;
        }

        /// <summary>
        /// Returns the property name
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName {
            get => propertyName;
        }

        /// <summary>
        /// Returns the statement name
        /// </summary>
        /// <returns>statement name</returns>
        public string StatementName {
            get => statementName;
        }

        /// <summary>
        /// Returns the engine URI
        /// </summary>
        /// <returns>engine URI</returns>
        public string EngineURI {
            get => engineURI;
        }

        /// <summary>
        /// Returns the event type
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType {
            get => eventType;
        }
    }
} // end of namespace