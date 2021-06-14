///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.render;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class ConsoleOpEventPropertyRenderer : EventPropertyRenderer
    {
        public static readonly ConsoleOpEventPropertyRenderer INSTANCE = new ConsoleOpEventPropertyRenderer();

        public void Render(EventPropertyRendererContext context)
        {
            if (context.PropertyValue is object[])
            {
                context.StringBuilder.Append(CompatExtensions.RenderAny((object[]) context.PropertyValue));
            }
            else
            {
                context.DefaultRenderer.Render(context.PropertyValue, context.StringBuilder);
            }
        }
    }
} // end of namespace