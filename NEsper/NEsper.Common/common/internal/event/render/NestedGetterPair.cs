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
    ///     Value-object for rendering support of a nested property value.
    /// </summary>
    public class NestedGetterPair
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">for retrieving the value</param>
        /// <param name="name">property name</param>
        /// <param name="metadata">the nested properties metadata</param>
        /// <param name="isArray">indicates whether this is an indexed property</param>
        public NestedGetterPair(EventPropertyGetter getter, string name, RendererMeta metadata, bool isArray)
        {
            Getter = getter;
            Name = name;
            Metadata = metadata;
            IsArray = isArray;
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
        ///     Returns the nested property's metadata.
        /// </summary>
        /// <returns>metadata</returns>
        public RendererMeta Metadata { get; }

        /// <summary>
        ///     Returns true if an indexed nested property.
        /// </summary>
        /// <returns>indicator whether indexed</returns>
        public bool IsArray { get; }
    }
} // end of namespace