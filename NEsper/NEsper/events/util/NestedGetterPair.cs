///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Value-object for rendering support of a nested property value.
    /// </summary>
    public class NestedGetterPair
    {
        private readonly String name;
        private readonly EventPropertyGetter getter;
        private readonly RendererMeta metadata;
        private readonly bool isArray;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">for retrieving the value</param>
        /// <param name="name">property name</param>
        /// <param name="metadata">the nested properties metadata</param>
        /// <param name="isArray">indicates whether this is an indexed property</param>
        public NestedGetterPair(EventPropertyGetter getter, String name, RendererMeta metadata, bool isArray)
        {
            this.getter = getter;
            this.name = name;
            this.metadata = metadata;
            this.isArray = isArray;
        }

        /// <summary>
        /// Returns the getter.
        /// </summary>
        /// <returns>
        /// getter
        /// </returns>
        public EventPropertyGetter Getter
        {
            get { return getter; }
        }

        /// <summary>
        /// Returns the property name.
        /// </summary>
        /// <returns>
        /// property name
        /// </returns>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Returns the nested property's metadata.
        /// </summary>
        /// <returns>
        /// metadata
        /// </returns>
        public RendererMeta Metadata
        {
            get { return metadata; }
        }

        /// <summary>
        /// Returns true if an indexed nested property.
        /// </summary>
        /// <returns>
        /// indicator whether indexed
        /// </returns>
        public bool IsArray
        {
            get { return isArray; }
        }
    }
}
