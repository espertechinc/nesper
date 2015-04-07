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
    /// Represents a simple value in a schema.
    /// </summary>
    [Serializable]
    public class SchemaElementSimple : SchemaElement
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="namespace">namespace</param>
        /// <param name="type">is the simple element type</param>
        /// <param name="typeName">name of type</param>
        /// <param name="isArray">if unbound</param>
        /// <param name="fractionDigits">The fraction digits.</param>
        public SchemaElementSimple(String name,
                                   String @namespace,
                                   XmlSchemaSimpleType type,
                                   String typeName,
                                   bool isArray,
                                   int? fractionDigits)
        {
            Name = name;
            Namespace = @namespace;
            SimpleType = type;
            IsArray = isArray;
            TypeName = typeName;
        }

        /// <summary>
        /// Returns type.
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

        #region SchemaElement Members

        /// <summary>
        /// Returns element name.
        /// </summary>
        /// <returns>
        /// element name
        /// </returns>
        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public bool IsArray { get; private set; }

        public int? FractionDigits { get; private set; }

        #endregion

        public override String ToString()
        {
            return "Simple " + Namespace + " " + Name;
        }
    }
}
