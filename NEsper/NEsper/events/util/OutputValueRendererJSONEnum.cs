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
    /// Renderer for a String-value into JSON enum.
    /// </summary>
    public class OutputValueRendererJSONEnum : OutputValueRenderer
    {
        public void Render(Object o, StringBuilder buf)
        {
            buf.Append('"');
            OutputValueRendererJSONString.Enquote(o.ToString(), buf);
            buf.Append('"');
        }
    }
}
