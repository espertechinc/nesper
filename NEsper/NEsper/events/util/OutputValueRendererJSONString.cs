///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Renderer for a String-value into JSON strings.
    /// </summary>
    public class OutputValueRendererJSONString : OutputValueRenderer
    {
        public void Render(Object o, StringBuilder buf)
        {
            if (o == null)
            {
                buf.Append("null");
                return;
            }
    
            Enquote(o.ToString(), buf);
        }
    
        /// <summary>
        /// JSON-Enquote the passed string.
        /// </summary>
        /// <param name="s">string to enqoute</param>
        /// <param name="sb">buffer to populate</param>
        public static void Enquote(String s, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(s))
            {
                sb.Append("\"\"");
                return;
            }
    
            char c;
            int i;
            int len = s.Length;
            String t;
    
            sb.Append('"');
            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                if ((c == '\\') || (c == '"'))
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else if (c == '\b')
                {
                    sb.Append("\\b");
                }
                else if (c == '\t')
                {
                    sb.Append("\\t");
                }
                else if (c == '\n')
                {
                    sb.Append("\\n");
                }
                else if (c == '\f')
                {
                    sb.Append("\\f");
                }
                else if (c == '\r')
                {
                    sb.Append("\\r");
                }
                else
                {
                    if (c < ' ')
                    {
                        t = "000" + ((short) c).ToString("X2");
                        sb.Append("\\u");
                        sb.Append(t.Substring(t.Length - 4));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            sb.Append('"');
        }
    }
}
