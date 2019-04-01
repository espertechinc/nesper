///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    /// Schema element is a simple or complex element.
    /// </summary>
    public interface SchemaElement : SchemaItem
    {
        /// <summary>
        /// Returns the namespace.
        /// </summary>
        /// <returns>
        /// namespace
        /// </returns>
        string Namespace { get; }

        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        string Name { get; }

        /// <summary>
        /// Returns true for unbounded or max>1
        /// </summary>
        /// <returns>
        /// array indicator
        /// </returns>
        bool IsArray { get; }
    }
}