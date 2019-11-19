///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Service provider interface for internal use for event types.
    /// </summary>
    public interface EventTypeSPI : EventType
    {
        /// <summary>
        ///     Returns the writable properties.
        /// </summary>
        /// <returns>properties that can be written</returns>
        EventPropertyDescriptor[] WriteableProperties { get; }

        /// <summary>
        ///     Returns a reader for reading all properties of an event. This is completely optional
        ///     and need only be implemented for performance.
        /// </summary>
        /// <returns>reader</returns>
        EventBeanReader Reader { get; }

        /// <summary>
        ///     Return a writer for writing a single property value.
        /// </summary>
        /// <param name="propertyName">to write to</param>
        /// <returns>null or writer if writable</returns>
        EventPropertyWriterSPI GetWriter(string propertyName);

        /// <summary>
        ///     Returns the descriptor for a writable property.
        /// </summary>
        /// <param name="propertyName">to get descriptor for</param>
        /// <returns>descriptor</returns>
        EventPropertyDescriptor GetWritableProperty(string propertyName);

        /// <summary>
        ///     Returns the copy method, considering only the attached properties for a write operation onto the copy
        /// </summary>
        /// <param name="properties">to write after copy</param>
        /// <returns>copy method</returns>
        EventBeanCopyMethodForge GetCopyMethodForge(string[] properties);

        /// <summary>
        ///     Returns the write for writing a set of properties.
        /// </summary>
        /// <param name="properties">to write</param>
        /// <returns>writer</returns>
        EventBeanWriter GetWriter(string[] properties);

        ExprValidationException EqualsCompareType(EventType eventType);

        EventPropertyGetterSPI GetGetterSPI(string propertyExpression);

        EventPropertyGetterMappedSPI GetGetterMappedSPI(string propertyName);

        EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string propertyName);

        void SetMetadataId(
            long publicId,
            long protectedId);
    }
} // end of namespace