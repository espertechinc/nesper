///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Represents a XSD schema or other metadata for a class of XML documents.
    /// </summary>
    public class SchemaModel
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="components">the top level components.</param>
        /// <param name="namespaces">list of namespaces</param>
        public SchemaModel(
            IList<SchemaElementComplex> components,
            IList<string> namespaces)
        {
            Components = components;
            Namespaces = namespaces;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="component">top level component</param>
        /// <param name="namespaces">list of namespaces</param>
        public SchemaModel(
            SchemaElementComplex component,
            IList<string> namespaces)
        {
            Components = new List<SchemaElementComplex>(1);
            Components.Add(component);
            Namespaces = namespaces;
        }

        /// <summary>
        ///     Returns top-level components.
        /// </summary>
        /// <returns>
        ///     components
        /// </returns>
        public IList<SchemaElementComplex> Components { get; }

        /// <summary>
        ///     Returns namespaces.
        /// </summary>
        /// <returns>
        ///     namespaces
        /// </returns>
        public IList<string> Namespaces { get; }
    }
}