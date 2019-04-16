///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
            if (constant == null)
            {
                return("null");
            }

            if ((constant is String) ||
                (constant is Char))
            {
                return '\"' + constant.ToString() + '\"';
            }
            else if (constant is Int64)
            {
                return string.Format("{0}L", constant);
            }
            else if (constant is Double)
            {
                double dvalue = (double)constant;
                double scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    return string.Format("{0:F1}d", dvalue);
                }
                else
                {
                    return string.Format("{0}", dvalue);
                }
            }
            else if (constant is Single)
            {
                double dvalue = (float)constant;
                double scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    return string.Format("{0:F1}f", dvalue);
                }
                else
                {
                    return string.Format("{0}f", dvalue);
                }
            }
            else if (constant is Decimal)
            {
                decimal dvalue = (decimal)constant;
                decimal scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    return string.Format("{0:F1}m", dvalue);
                }
                else
                {
                    return string.Format("{0}m", dvalue);
                }
            }
            else if (constant is Boolean)
            {
                return(constant.ToString().ToLower());
            }
            else
            {
                return(constant.ToString());
            }
        }

        /// <summary>Renders a constant as an EPL. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="constant">to render</param>
        public static void RenderEPL(TextWriter writer, Object constant)
        {
            if (constant == null)
            {
                writer.Write("null");
                return;
            }

            if ((constant is String) ||
                (constant is Char))
            {
                writer.Write('\"');
                writer.Write(constant.ToString());
                writer.Write('\"');
            }
            else if (constant is Int64)
            {
                writer.Write("{0}L", constant);
            }
            else if (constant is Double)
            {
                double dvalue = (double)constant;
                double scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    writer.Write("{0:F1}d", dvalue);
                }
                else
                {
                    writer.Write("{0}", dvalue);
                }
            }
            else if (constant is Single)
            {
                double dvalue = (float)constant;
                double scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    writer.Write("{0:F1}f", dvalue);
                }
                else
                {
                    writer.Write("{0}f", dvalue);
                }
            } 
            else if (constant is Decimal)
            {
                decimal dvalue = (decimal)constant;
                decimal scrubbed = Math.Floor(dvalue);
                if (dvalue == scrubbed)
                {
                    writer.Write("{0:F1}m", dvalue);
                }
                else
                {
                    writer.Write("{0}m", dvalue);
                }
            }
            else if (constant is Boolean)
            {
                writer.Write(constant.ToString().ToLower());
            }
            else
            {
                writer.Write(constant.ToString());
            }
        }
    }
}
