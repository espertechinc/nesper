///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Interface for context dimension descriptors.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<ContextDescriptor>))]
    public interface ContextDescriptor
    {
        /// <summary>
        /// Format as EPL.
        /// </summary>
        /// <param name="writer">output</param>
        /// <param name="formatter">formatter</param>
        void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter);
    }
} // end of namespace