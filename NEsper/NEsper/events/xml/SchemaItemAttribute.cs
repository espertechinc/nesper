///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml.Schema;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Represents an attribute in a schema.
    /// </summary>
    [Serializable]
    public class SchemaItemAttribute : SchemaItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namespace">namespace</param>
        /// <param name="name">name</param>
        /// <param name="type">attribute type</param>
        /// <param name="typeName">attribute type name</param>
        public SchemaItemAttribute(String @namespace, String name, XmlSchemaSimpleType type, String typeName)
        {
            Name = name;
            Namespace = @namespace;
            SimpleType = type;
            TypeName = typeName;
        }

        /// <summary>
        /// Returns the namespace.
        /// </summary>
        /// <returns>
        /// namespace
        /// </returns>
        public string Namespace { get; private set; }

        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the type.
        /// </summary>
        /// <returns>
        /// type
        /// </returns>
        public XmlSchemaSimpleType SimpleType { get; private set; }

        /// <summary>
        /// Returns the type name.
        /// </summary>
        /// <returns>
        /// type name
        /// </returns>
        public string TypeName { get; private set; }

        public override String ToString()
        {
            return "Attribute " + Namespace + " " + Name;
        }
    }
}
