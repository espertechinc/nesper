///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    /// XML rendering options.
    /// </summary>
    public class XMLRenderingOptions
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public XMLRenderingOptions()
        {
            PreventLooping = true;
            IsDefaultAsAttribute = false;
        }

        /// <summary>
        /// Indicator whether to prevent looping, by default set to true. Set to false to
        /// allow looping in the case where nested properties may refer to themselves, for
        /// example.
        /// <para/>
        /// The algorithm to control looping considers the combination of event type and
        /// property name for each level of nested property.
        /// </summary>
        /// <returns>
        /// indicator whether the rendering algorithm prevents looping behavior
        /// </returns>
        public bool PreventLooping { get; set; }

        /// <summary>
        /// Indicator whether simple properties are rendered as attributes, this setting is
        /// false by default thereby simple properties are rendered as elements.
        /// </summary>
        /// <returns>
        /// true for simple properties rendered as attributes
        /// </returns>
        public bool IsDefaultAsAttribute { get; set; }

        /// <summary>
        /// Gets or sets the event property renderer to use.
        /// </summary>
        /// <value>The renderer.</value>
        public EventPropertyRenderer Renderer { get; set; }
    }
}
