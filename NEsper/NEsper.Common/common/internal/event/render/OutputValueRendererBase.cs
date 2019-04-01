///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>
    /// Renderer for a Object values that can simply be output via to-string.
    /// </summary>
    public class OutputValueRendererBase : OutputValueRenderer
    {
        public void Render(Object o, StringBuilder buf)
        {
            if (o == null)
            {
                buf.Append("null");
                return;
            }

            if ((o is decimal) ||
                (o is decimal?) ||
                (o is double) ||
                (o is double?) ||
                (o is float) ||
                (o is float?))
            {
                var text = o.ToString();
                buf.Append(text);
                if (text.IndexOf('.') == -1)
                {
                    buf.Append(".0");
                }
            }
            else if (o is DateTimeOffset)
            {
                var dateTime = (DateTimeOffset)o;
                var dateOnly = dateTime.Date;
                if (dateTime == dateOnly)
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd"));
                }
                else if (dateTime.Millisecond == 0)
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd hh:mm:ss"));
                }
                else
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd hh:mm:ss.ffff"));
                }
            }
            else if (o is DateTime)
            {
                var dateTime = (DateTime) o;
                var dateOnly = dateTime.Date;
                if (dateTime == dateOnly)
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd"));
                }
                else if (dateTime.Millisecond == 0)
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd hh:mm:ss"));
                }
                else
                {
                    buf.Append(dateTime.ToString("yyyy-MM-dd hh:mm:ss.ffff"));
                }
            }
            else if (o is bool)
            {
                buf.Append(o.ToString().ToLowerInvariant());
            }
            else if (o.GetType().IsEnum)
            {
                buf.AppendFormat("{0}", o);
            }
            else
            {
                buf.Append(o.ToString());
            }
        }
    }
}