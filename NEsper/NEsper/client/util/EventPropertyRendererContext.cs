///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.events.util;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Context for use with the <seealso cref="EventPropertyRenderer" /> interface for use with the JSON 
    /// or XML event renderes to handle custom event property rendering. 
    /// <para>
    /// Do not retain a handle to the renderer context as this object changes for each event property.
    /// </para>
    /// </summary>
    public class EventPropertyRendererContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="eventType">event type</param>
        /// <param name="jsonFormatted">bool if JSON formatted</param>
        public EventPropertyRendererContext(EventType eventType, bool jsonFormatted)
        {
            EventType = eventType;
            IsJsonFormatted = jsonFormatted;
        }

        /// <summary>Returns the property name to be rendered. </summary>
        /// <value>property name</value>
        public string PropertyName { get; set; }

        /// <summary>Returns the property value. </summary>
        /// <value>value</value>
        public object PropertyValue { get; set; }

        /// <summary>Returns the output value default renderer. </summary>
        /// <value>renderer</value>
        public OutputValueRenderer DefaultRenderer { get; set; }

        /// <summary>Sets the string builer </summary>
        /// <param name="stringBuilder">to set</param>
        public void SetStringBuilderAndReset(StringBuilder stringBuilder) {
            StringBuilder = stringBuilder;
            MappedPropertyKey = null;
            IndexedPropertyIndex = null;
        }

        /// <summary>Returns the string builder. </summary>
        /// <value>string builder to use</value>
        public StringBuilder StringBuilder { get; private set; }

        /// <summary>Returns the event type </summary>
        /// <value>event type</value>
        public EventType EventType { get; private set; }

        /// <summary>Returns the index for indexed properties. </summary>
        /// <value>property index</value>
        public int? IndexedPropertyIndex { get; set; }

        /// <summary>Returns the map key for mapped properties </summary>
        /// <value>map key</value>
        public string MappedPropertyKey { get; set; }

        /// <summary>Returns true for JSON formatted. </summary>
        /// <value>indicator</value>
        public bool IsJsonFormatted { get; private set; }

        /// <summary>Copies context. </summary>
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
}
