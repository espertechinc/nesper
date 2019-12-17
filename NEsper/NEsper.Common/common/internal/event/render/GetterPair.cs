///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>
    ///     Value-object for rendering support of a simple property value (non-nested).
    /// </summary>
    public class GetterPair
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">for retrieving the value</param>
        /// <param name="name">property name</param>
        /// <param name="output">for rendering the getter result</param>
        public GetterPair(
            EventPropertyGetter getter,
            string name,
            OutputValueRenderer output)
        {
            Getter = getter;
            Name = name;
            Output = output;
        }

        /// <summary>
        ///     Returns the getter.
        /// </summary>
        /// <returns>getter</returns>
        public EventPropertyGetter Getter { get; }

        /// <summary>
        ///     Returns the property name.
        /// </summary>
        /// <returns>property name</returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the renderer for the getter return value.
        /// </summary>
        /// <returns>renderer for result value</returns>
        public OutputValueRenderer Output { get; }
    }
} // end of namespace