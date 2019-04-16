///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.render;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>
    ///     Options for use by <seealso cref="RendererMeta" /> with rendering metadata.
    /// </summary>
    public class RendererMetaOptions
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="preventLooping">true to prevent looping</param>
        /// <param name="isXmlOutput">true for XML output</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="rendererContext">The renderer context.</param>
        public RendererMetaOptions(
            bool preventLooping,
            bool isXmlOutput,
            EventPropertyRenderer renderer,
            EventPropertyRendererContext rendererContext)
        {
            PreventLooping = preventLooping;
            IsXmlOutput = isXmlOutput;
            Renderer = renderer;
            RendererContext = rendererContext;
        }

        /// <summary>
        ///     Returns true to prevent looping.
        /// </summary>
        /// <returns>
        ///     prevent looping indicator
        /// </returns>
        public bool PreventLooping { get; }

        /// <summary>
        ///     Returns true for XML output.
        /// </summary>
        /// <returns>
        ///     XML output flag
        /// </returns>
        public bool IsXmlOutput { get; }

        /// <summary>
        ///     Gets or sets the renderer.
        /// </summary>
        /// <value>The renderer.</value>
        public EventPropertyRenderer Renderer { get; }

        /// <summary>
        ///     Gets or sets the renderer context.
        /// </summary>
        /// <value>The renderer context.</value>
        public EventPropertyRendererContext RendererContext { get; }
    }
}