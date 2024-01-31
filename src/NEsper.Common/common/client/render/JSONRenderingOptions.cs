///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    /// JSON rendering options.
    /// </summary>
    public class JSONRenderingOptions
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public JSONRenderingOptions()
        {
            PreventLooping = true;
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
        /// Gets or sets the event property renderer to use.
        /// </summary>
        /// <value>The renderer.</value>
        public EventPropertyRenderer Renderer { get; set; }
    }
}