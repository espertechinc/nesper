///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.primitive
{
    public static class JsonDeserializerPrimitives
    {
        public static EPException HandleNumberException(
            Type type,
            JsonElement value,
            FormatException ex)
        {
            var innerMsg = ex.Message == null ? "" : " " + ex.Message.Replace("For", "for");
            return new EPException(
                "Failed to parse json value as a " +
                type.Name +
                "-type from value '" +
                value +
                "': NumberFormatException" +
                innerMsg,
                ex);
        }

        public static EPException HandleBooleanException(JsonElement value)
        {
            return new EPException("Failed to parse json value as a boolean-type from value '" + value + "'");
        }

        public static EPException HandleParseException(
            Type type,
            JsonElement value,
            Exception ex)
        {
            var innerMsg = ex.Message;
            return new EPException(
                "Failed to parse json value as a " + type.Name + "-type from value '" + value + "': " + innerMsg,
                ex);
        }
    }
} // end of namespace