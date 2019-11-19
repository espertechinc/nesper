///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    /// Renderer for an event into the XML textual format.
    /// <para>
    /// A renderer is dedicated to rendering only a certain type of events and subtypes
    /// of that type, as the render cache type metadata and prepares structures to
    /// enable fast rendering.
    /// </para>
    /// <para>
    /// For rendering events of different types, use a quick-access method in <seealso cref="EventRenderer"/>.
    /// </para>
    /// </summary>
    public interface XMLEventRenderer
    {
        /// <summary>
        /// RenderAny a given event in the XML format.
        /// </summary>
        /// <param name="rootElementName">the name of the root element, may include namespace information</param>
        /// <param name="theEvent">the event to render</param>
        /// <returns>
        /// XML formatted text
        /// </returns>
        string Render(
            string rootElementName,
            EventBean theEvent);
    }
}