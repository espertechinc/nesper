///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    ///     For Avro types for widening objects to Avro record values, see <seealso cref="ObjectValueTypeWidenerFactory" />
    /// </summary>
    public class ObjectValueTypeWidenerFactoryContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="eventType">event type</param>
        /// <param name="statementName">statement name</param>
        public ObjectValueTypeWidenerFactoryContext(
            Type clazz,
            string propertyName,
            EventType eventType,
            string statementName)
        {
            Clazz = clazz;
            PropertyName = propertyName;
            EventType = eventType;
            StatementName = statementName;
        }

        /// <summary>
        ///     Returns the class
        /// </summary>
        /// <returns>class</returns>
        public Type Clazz { get; }

        /// <summary>
        ///     Returns the property name
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns the statement name
        /// </summary>
        /// <returns>statement name</returns>
        public string StatementName { get; }

        /// <summary>
        ///     Returns the event type
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType { get; }
    }
} // end of namespace