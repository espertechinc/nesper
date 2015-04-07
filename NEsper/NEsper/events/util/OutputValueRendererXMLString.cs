///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Renderer for a String-value into XML strings.
    /// </summary>
    public class OutputValueRendererXMLString : OutputValueRenderer
    {
        public void Render(Object o, StringBuilder buf)
        {
            if (o == null)
            {
                buf.Append("null");
                return;
            }
    
            XmlEncode(o.ToString(), buf, true);
        }
    
        /// <summary>
        /// XML-Encode the passed string.
        /// </summary>
        /// <param name="s">string to encode</param>
        /// <param name="sb">string buffer to populate</param>
        /// <param name="isEncodeSpecialChar">true for encoding of special characters below ' ', false for leaving special chars</param>
        public static void XmlEncode(String s, StringBuilder sb, bool isEncodeSpecialChar)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
    
            char c;
            int i;
            int len = s.Length;
            String t;
    
            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                // replace literal values with entities
    
                if (c == '&')
                {
                    sb.Append("&amp;");
                }
                else if (c == '<')
                {
                    sb.Append("&lt;");
                }
                else if (c == '>')
                {
                    sb.Append("&gt;");
                }
                else if (c == '\'')
                {
                    sb.Append("&apos;");
                }
                else if (c == '\"')
                {
                    sb.Append("&quot;");
                }
                else
                {
                    if ((c < ' ') && (isEncodeSpecialChar))
                    {
                        t = "000" + ((short) c).ToString("x2");
                        sb.Append("\\u");
                        sb.Append(t.Substring(t.Length - 4));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
        }
    }
}
