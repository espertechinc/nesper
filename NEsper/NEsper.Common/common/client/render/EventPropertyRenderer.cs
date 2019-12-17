///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    /// Interface for use with the JSON or XML event renderes to handle custom event property rendering.
    /// <para>Implementations of this interface are called for each event property and may utilize the context object provided to render the event property value to a string. </para>
    /// <para>The context itself contains a reference to the default renderer that can be delegated to for properties that use the default rendering.</para>
    /// <para>Do not retain a handle to the renderer context as the context object changes for each event property.</para>
    /// </summary>
    public interface EventPropertyRenderer
    {
        /// <summary>RenderAny an event property. </summary>
        /// <param name="context">provides information about the property</param>
        void Render(EventPropertyRendererContext context);
    }
}