///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonEndValueForgeUtil
    {
        public static EPException HandleNumberException(
            string name,
            Type type,
            string value,
            FormatException ex)
        {
            var innerMsg = ex.Message == null ? "" : " " + ex.Message.Replace("For", "for");
            return new EPException(
                "Failed to parse json member name '" + name + "' as a " + type.Name + "-type from value '" + value + "': NumberFormatException" + innerMsg,
                ex);
        }

        public static EPException HandleBooleanException(
            string name,
            string value)
        {
            return new EPException("Failed to parse json member name '" + name + "' as a boolean-type from value '" + value + "'");
        }

        public static EPException HandleParseException(
            string name,
            Type type,
            string value,
            Exception ex)
        {
            var innerMsg = ex.Message == null ? "" : ex.Message;
            return new EPException(
                "Failed to parse json member name '" + name + "' as a " + type.Name + "-type from value '" + value + "': " + innerMsg,
                ex);
        }
    }
} // end of namespace