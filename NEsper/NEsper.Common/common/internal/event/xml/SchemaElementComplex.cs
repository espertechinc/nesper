///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Represents a complex element possibly with attributes, simple elements, other
    ///     complex child elements.
    /// </summary>
    [Serializable]
    public class SchemaElementComplex : SchemaElement
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">the element name</param>
        /// <param name="namespace">the element namespace</param>
        /// <param name="attributes">the attributes or empty if none</param>
        /// <param name="children">the child complex elements or empty if none</param>
        /// <param name="simpleElements">the simple elements or empty if none</param>
        /// <param name="isArray">if unbound or max&gt;1</param>
        /// <param name="optionalSimpleType">if the element does itself have a type</param>
        /// <param name="optionalSimpleTypeName">if the element does itself have a type</param>
        public SchemaElementComplex(
            string name,
            string @namespace,
            IList<SchemaItemAttribute> attributes,
            IList<SchemaElementComplex> children,
            IList<SchemaElementSimple> simpleElements,
            bool isArray,
            XmlSchemaSimpleType optionalSimpleType,
            XmlQualifiedName optionalSimpleTypeName)
        {
            Name = name;
            Namespace = @namespace;
            Attributes = attributes;
            ComplexElements = children;
            SimpleElements = simpleElements;
            IsArray = isArray;
            OptionalSimpleType = optionalSimpleType;
            OptionalSimpleTypeName = optionalSimpleTypeName;
        }

        /// <summary>
        ///     Returns attributes.
        /// </summary>
        /// <returns>
        ///     attributes
        /// </returns>
        public IList<SchemaItemAttribute> Attributes { get; }

        /// <summary>
        ///     Returns complex child elements.
        /// </summary>
        /// <returns>
        ///     attributes
        /// </returns>
        public IList<SchemaElementComplex> ComplexElements { get; }

        /// <summary>
        ///     Returns simple child elements.
        /// </summary>
        /// <returns>
        ///     simple child elements
        /// </returns>
        public IList<SchemaElementSimple> SimpleElements { get; }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return
                string.Format(
                    "SchemaElementComplex{{Attributes: {0}, ComplexElements: {1}, SimpleElements: {2}, Name: {3}, Namespace: {4}, IsArray: {5}, OptionalSimpleType: {6}, OptionalSimpleTypeName: {7}}}",
                    Attributes,
                    ComplexElements,
                    SimpleElements,
                    Name,
                    Namespace,
                    IsArray,
                    OptionalSimpleType,
                    OptionalSimpleTypeName);
        }

        #region SchemaElement Members

        /// <summary>
        ///     Returns the name.
        /// </summary>
        /// <returns>
        ///     name
        /// </returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the namespace of the element.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     namespace
        /// </returns>
        public string Namespace { get; }

        /// <summary>
        ///     Returns true if unbound or max greater one.
        /// </summary>
        /// <returns>
        ///     true if array
        /// </returns>
        public bool IsArray { get; }

        /// <summary>
        ///     Gets or sets the type of the optional simple.  If not null, then the
        ///     complex element itself has a type defined for it.
        /// </summary>
        /// <value>The type of the optional simple.</value>
        public XmlSchemaSimpleType OptionalSimpleType { get; set; }

        /// <summary>
        ///     Gets or sets the name of the optional simple type.  If not null then
        ///     the complex element itself has a type defined for it.
        /// </summary>
        /// <value>The name of the optional simple type.</value>
        public XmlQualifiedName OptionalSimpleTypeName { get; set; }

        #endregion
    }
}