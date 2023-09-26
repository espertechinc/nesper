///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Helper methods for use by the statement object model.
    /// </summary>
    public static class EPStatementObjectModelHelper
    {
        public static string RenderValue(this object constant)
        {
            if (constant == null) {
                return "null";
            }

            if (constant is string ||
                constant is char) {
                return '\"' + constant.ToString() + '\"';
            }
            else if (constant is long) {
                return $"{constant}L";
            }
            else if (constant is double d) {
                var scrubbed = Math.Floor(d);
                if (d == scrubbed) {
                    return $"{d:F1}d";
                }
                else {
                    return $"{d}";
                }
            }
            else if (constant is float f) {
                double dvalue = f;
                var scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed) {
                    return $"{dvalue:F1}f";
                }
                else {
                    return $"{dvalue}f";
                }
            }
            else if (constant is decimal dvalue) {
                var scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed) {
                    return $"{dvalue:F1}m";
                }
                else {
                    return $"{dvalue}m";
                }
            }
            else if (constant is bool) {
                return constant.ToString().ToLower();
            }
            else {
                return constant.ToString();
            }
        }

        /// <summary>Renders a constant as an EPL. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="constant">to render</param>
        public static void RenderEPL(
            TextWriter writer,
            object constant)
        {
            if (constant == null) {
                writer.Write("null");
                return;
            }

            if (constant is string ||
                constant is char) {
                writer.Write('\"');
                writer.Write(constant.ToString());
                writer.Write('\"');
            }
            else if (constant is long) {
                writer.Write("{0}L", constant);
            }
            else if (constant is double d) {
                var scrubbed = Math.Floor(d);
                if (d == scrubbed) {
                    writer.Write("{0:F1}d", d);
                }
                else {
                    writer.Write("{0}", d);
                }
            }
            else if (constant is float f) {
                double dvalue = f;
                var scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed) {
                    writer.Write("{0:F1}f", dvalue);
                }
                else {
                    writer.Write("{0}f", dvalue);
                }
            }
            else if (constant is decimal dvalue) {
                var scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed) {
                    writer.Write("{0:F1}m", dvalue);
                }
                else {
                    writer.Write("{0}m", dvalue);
                }
            }
            else if (constant is bool) {
                writer.Write(constant.ToString().ToLower());
            }
            else {
                writer.Write(constant.ToString());
            }
        }
    }
}