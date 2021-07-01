///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Provides the namespace context information for compiling XPath expressions.
    /// </summary>
    public class XPathNamespaceContext : XsltContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public XPathNamespaceContext()
        {
            base.AddNamespace(
                XMLConstants.XML_NS_PREFIX,
                XMLConstants.XML_NS_URI);

            // Prefix "xmlns" is reserved for use by XML

            //base.AddNamespace(
            //    XMLConstants.XMLNS_ATTRIBUTE,
            //    XMLConstants.XMLNS_ATTRIBUTE_NS_URI);
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether to include white space nodes in the output.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     true to check white space nodes in the source document for inclusion in the output; false to not evaluate white
        ///     space nodes. The default is true.
        /// </returns>
        public override bool Whitespace => false;

        /// <summary>
        ///     Sets the default namespace.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetDefaultNamespace(string value)
        {
            var prefix = XMLConstants.DEFAULT_NS_PREFIX;
            if (HasNamespace(prefix)) {
                RemoveNamespace(prefix, LookupNamespace(prefix));
            }

            AddNamespace(prefix, value);
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            var builder =
                new StringBuilder("XPathNamespaceContext default namespace '" + DefaultNamespace + "' maps {");
            var delimiter = "";

            var namespaceTable = GetNamespacesInScope(XmlNamespaceScope.All);
            foreach (var entry in namespaceTable) {
                builder.Append(delimiter);
                builder.Append(entry.Key);
                builder.Append("=");
                builder.Append(entry.Value);
                delimiter = ",";
            }

            builder.Append("}");
            return builder.ToString();
        }

        /// <summary>
        ///     When overridden in a derived class, resolves a variable reference and returns an
        ///     <see cref="T:System.Xml.Xsl.IXsltContextVariable" /> representing the variable.
        /// </summary>
        /// <param name="prefix">The prefix of the variable as it appears in the XPath expression.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        ///     An <see cref="T:System.Xml.Xsl.IXsltContextVariable" /> representing the variable at runtime.
        /// </returns>
        public override IXsltContextVariable ResolveVariable(
            string prefix,
            string name)
        {
            return null;
        }

        /// <summary>
        ///     When overridden in a derived class, resolves a function reference and returns an
        ///     <see cref="T:System.Xml.Xsl.IXsltContextFunction" /> representing the function. The
        ///     <see cref="T:System.Xml.Xsl.IXsltContextFunction" /> is used at execution time to get the return value of the
        ///     function.
        /// </summary>
        /// <param name="prefix">The prefix of the function as it appears in the XPath expression.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="argTypes">
        ///     An array of argument types for the function being resolved. This allows you to select between
        ///     methods with the same name (for example, overloaded methods).
        /// </param>
        /// <returns>
        ///     An <see cref="T:System.Xml.Xsl.IXsltContextFunction" /> representing the function.
        /// </returns>
        public override IXsltContextFunction ResolveFunction(
            string prefix,
            string name,
            XPathResultType[] argTypes)
        {
            return null;
        }

        /// <summary>
        ///     When overridden in a derived class, evaluates whether to preserve white space nodes or strip them for the given
        ///     context.
        /// </summary>
        /// <param name="node">The white space node that is to be preserved or stripped in the current context.</param>
        /// <returns>
        ///     Returns true if the white space is to be preserved or false if the white space is to be stripped.
        /// </returns>
        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return false;
        }

        /// <summary>
        ///     When overridden in a derived class, compares the base Uniform Resource Identifiers (URIs) of two documents based
        ///     upon the order the documents were loaded by the XSLT processor (that is, the
        ///     <see cref="T:System.Xml.Xsl.XslTransform" /> class).
        /// </summary>
        /// <param name="baseUri">The base URI of the first document to compare.</param>
        /// <param name="nextbaseUri">The base URI of the second document to compare.</param>
        /// <returns>
        ///     An integer value describing the relative order of the two base URIs: -1 if <paramref name="baseUri" /> occurs
        ///     before <paramref name="nextbaseUri" />; 0 if the two base URIs are identical; and 1 if <paramref name="baseUri" />
        ///     occurs after <paramref name="nextbaseUri" />.
        /// </returns>
        public override int CompareDocument(
            string baseUri,
            string nextbaseUri)
        {
            throw new NotSupportedException();
        }
    }

    public delegate XPathNamespaceContext XPathNamespaceContextFactory();
}