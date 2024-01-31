///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    /// For use with high-availability and scale-out only, provider for serde (serializer and deserializer) information
    /// to the compiler.
    /// </summary>
    public interface SerdeProvider
    {
        /// <summary>
        /// Returns a serde provision or null if none can be determined.
        /// </summary>
        /// <param name="context">provider context object</param>
        /// <returns>null for no serde available, or the serde provider descriptor for the compiler</returns>
        SerdeProvision ResolveSerdeForClass(SerdeProviderContextClass context);

        /// <summary>
        /// Returns a serde for a map or object-array event type or null for using the default serde
        /// </summary>
        /// <param name="context">provides information about the event type</param>
        /// <returns>null to use the default runtime serde, or the serde provider descriptor for the compiler</returns>
        SerdeProvision ResolveSerdeForEventType(SerdeProviderEventTypeContext context);
    }
}