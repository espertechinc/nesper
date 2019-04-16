///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.common.@internal.@event.render;

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    ///     Context for use with the <seealso cref="EventPropertyRenderer" /> interface for use with the JSON or XML event
    ///     renders to handle custom event property rendering.
    ///     <para>Do not retain a handle to the renderer context as this object changes for each event property.</para>
    /// </summary>
    public class EventPropertyRendererContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="jsonFormatted">boolean if JSON formatted</param>
        public EventPropertyRendererContext(
            EventType eventType,
            bool jsonFormatted)
        {
            EventType = eventType;
            IsJsonFormatted = jsonFormatted;
        }

        /// <summary>
        ///     Returns the property name to be rendered.
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName { get; set; }

        /// <summary>
        ///     Returns the property value.
        /// </summary>
        /// <returns>value</returns>
        public object PropertyValue { get; set; }

        /// <summary>
        ///     Returns the output value default renderer.
        /// </summary>
        /// <returns>renderer</returns>
        public OutputValueRenderer DefaultRenderer { get; set; }

        /// <summary>
        ///     Returns the string builder.
        /// </summary>
        /// <returns>string builder to use</returns>
        public StringBuilder StringBuilder { get; private set; }

        /// <summary>
        ///     Returns the event type
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType { get; }

        /// <summary>
        ///     Returns the index for indexed properties.
        /// </summary>
        /// <returns>property index</returns>
        public int? IndexedPropertyIndex { get; set; }

        /// <summary>
        ///     Returns the map key for mapped properties
        /// </summary>
        /// <returns>map key</returns>
        public string MappedPropertyKey { get; set; }

        /// <summary>
        ///     Returns true for JSON formatted.
        /// </summary>
        /// <returns>indicator</returns>
        public bool IsJsonFormatted { get; }

        /// <summary>
        ///     Sets the string builer
        /// </summary>
        /// <param name="stringBuilder">to set</param>
        public void SetStringBuilderAndReset(StringBuilder stringBuilder)
        {
            StringBuilder = stringBuilder;
            MappedPropertyKey = null;
            IndexedPropertyIndex = null;
        }

        /// <summary>
        ///     Copies context.
        /// </summary>
        /// <returns>copy</returns>
        public EventPropertyRendererContext Copy()
        {
            var copy = new EventPropertyRendererContext(EventType, IsJsonFormatted);
            copy.MappedPropertyKey = MappedPropertyKey;
            copy.IndexedPropertyIndex = IndexedPropertyIndex;
            copy.DefaultRenderer = DefaultRenderer;
            copy.PropertyName = PropertyName;
            copy.PropertyValue = PropertyValue;
            return copy;
        }
    }
} // end of namespace