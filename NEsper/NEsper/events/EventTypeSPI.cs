///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Service provider interface for internal use for event types.
    /// </summary>
    public interface EventTypeSPI : EventType
    {
        /// <summary>
        /// Returns the type metadata.
        /// </summary>
        /// <returns>
        /// type metadata
        /// </returns>
        EventTypeMetadata Metadata { get; }

        /// <summary>Return a writer for writing a single property value. </summary>
        /// <param name="propertyName">to write to</param>
        /// <returns>null or writer if writable</returns>
        EventPropertyWriter GetWriter(string propertyName);

        /// <summary>Returns the writable properties. </summary>
        /// <value>properties that can be written</value>
        EventPropertyDescriptor[] WriteableProperties { get; }

        /// <summary>Returns the descriptor for a writable property. </summary>
        /// <param name="propertyName">to Get descriptor for</param>
        /// <returns>descriptor</returns>
        EventPropertyDescriptor GetWritableProperty(string propertyName);

        /// <summary>Returns the copy method, considering only the attached properties for a write operation onto the copy </summary>
        /// <param name="properties">to write after copy</param>
        /// <returns>copy method</returns>
        EventBeanCopyMethod GetCopyMethod(string[] properties);

        /// <summary>Returns the write for writing a set of properties. </summary>
        /// <param name="properties">to write</param>
        /// <returns>writer</returns>
        EventBeanWriter GetWriter(string[] properties);

        /// <summary>Returns a reader for reading all properties of an event. This is completely optional and need only be implemented for performance. </summary>
        /// <value>reader</value>
        EventBeanReader Reader { get; }

        bool EqualsCompareType(EventType eventType);

        EventPropertyGetterSPI GetGetterSPI(string propertyExpression);
    }
}
