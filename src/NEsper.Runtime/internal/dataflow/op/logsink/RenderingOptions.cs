///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.render;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class RenderingOptions
    {
        static RenderingOptions()
        {
            XmlOptions = new XMLRenderingOptions();
            XmlOptions.PreventLooping = true;
            XmlOptions.Renderer = ConsoleOpEventPropertyRenderer.INSTANCE;

            JsonOptions = new JSONRenderingOptions();
            JsonOptions.PreventLooping = true;
            JsonOptions.Renderer = ConsoleOpEventPropertyRenderer.INSTANCE;
        }

        public static XMLRenderingOptions XmlOptions { get; set; }

        public static JSONRenderingOptions JsonOptions { get; set; }
    }
} // end of namespace