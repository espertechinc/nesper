///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>For rendering an output value returned by a property. </summary>
    public class OutputValueRendererFactory
    {
        private static readonly OutputValueRenderer JsonStringOutput = new OutputValueRendererJSONString();
        private static readonly OutputValueRenderer XmlStringOutput = new OutputValueRendererXMLString();
        private static readonly OutputValueRenderer JsonEnumOutput = new OutputValueRendererJSONString();
        private static readonly OutputValueRenderer BaseOutput = new OutputValueRendererBase();

        /// <summary>Returns a renderer for an output value. </summary>
        /// <param name="type">to render</param>
        /// <param name="options">options</param>
        /// <returns>renderer</returns>
        protected internal static OutputValueRenderer GetOutputValueRenderer(
            Type type,
            RendererMetaOptions options)
        {
            if (type.IsArray) {
                type = type.GetElementType();
            }

            if (type == typeof(string) || type == typeof(char?) || type == typeof(char) || type.IsEnum) {
                return options.IsXmlOutput ? XmlStringOutput : JsonStringOutput;
            }

            if (type.IsEnum || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsEnum)) {
                return options.IsXmlOutput ? BaseOutput : JsonEnumOutput;
            }

            return BaseOutput;
        }
    }
}