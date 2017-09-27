///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using com.espertech.esper.compat.xml;

namespace com.espertech.esper.events.xml
{
    public class ExtendedContext : XsltContext
    {
        private readonly XsltContext _baseContext;
        private readonly IXPathFunctionResolver _functionResolver;
        private readonly IXPathVariableResolver _variableResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedContext"/> class.
        /// </summary>
        /// <param name="baseContext">The base context.</param>
        /// <param name="functionResolver">The function resolver.</param>
        /// <param name="variableResolver">The variable resolver.</param>
        public ExtendedContext(XsltContext baseContext, IXPathFunctionResolver functionResolver, IXPathVariableResolver variableResolver)
        {
            _baseContext = baseContext;
            _functionResolver = functionResolver;
            _variableResolver = variableResolver;
        }

        /// <summary>
        /// Adds the given namespace to the collection.
        /// </summary>
        /// <param name="prefix">The prefix to associate with the namespace being added. Use String.EmptyFalse to add a default namespace.
        /// Note:
        /// If the
        /// <see cref="T:System.Xml.XmlNamespaceManager"/> will be used for resolving namespaces in an XML Path Language (XPath)
        /// expression, a prefix must be specified. If an XPath expression does not include a prefix, it is assumed that the namespace
        /// Uniform Resource Identifier (URI) is the empty namespace. For more information about XPath expressions and the
        /// <see cref="T:System.Xml.XmlNamespaceManager"/>, refer to the <see cref="M:System.Xml.XmlNode.SelectNodes(System.String)"/> and 
        /// <see cref="M:System.Xml.XPath.XPathExpression.SetContext(System.Xml.XmlNamespaceManager)"/> methods.
        /// </param>
        /// <param name="uri">The namespace to add.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The value for <paramref name="prefix"/> is "xml" or "xmlns".
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The value for <paramref name="prefix"/> or <paramref name="uri"/> is null.
        /// </exception>
        public override void AddNamespace(string prefix, string uri)
        {
            throw new ReadOnlyException();
        }

        /// <summary>
        /// When overridden in a derived class, compares the base Uniform Resource Identifiers (URIs) of two documents based upon the
        /// order the documents were loaded by the XSLT processor (that is, the <see cref="T:System.Xml.Xsl.XslTransform"/> class).
        /// </summary>
        /// <param name="baseUri">The base URI of the first document to compare.</param>
        /// <param name="nextbaseUri">The base URI of the second document to compare.</param>
        /// <returns>
        /// An integer value describing the relative order of the two base URIs: -1 if <paramref name="baseUri"/> occurs before <paramref name="nextbaseUri"/>; 0 if the two base URIs are identical; and 1 if <paramref name="baseUri"/> occurs after <paramref name="nextbaseUri"/>.
        /// </returns>
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return _baseContext.CompareDocument(baseUri, nextbaseUri);
        }

        /// <summary>
        /// Gets the namespace URI for the default namespace.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// Returns the namespace URI for the default namespace, or String.EmptyFalse if there is no default namespace.
        /// </returns>
        public override string DefaultNamespace
        {
            get { return _baseContext.DefaultNamespace; }
        }

        /// <summary>
        /// When overridden in a derived class, resolves a variable reference and returns an
        /// <see cref="T:System.Xml.Xsl.IXsltContextVariable"/> representing the variable.
        /// </summary>
        /// <param name="prefix">The prefix of the variable as it appears in the XPath expression.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// An <see cref="T:System.Xml.Xsl.IXsltContextVariable"/> representing the variable at runtime.
        /// </returns>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            if (_variableResolver != null) {
                var variable = _variableResolver.ResolveVariable(prefix, name);
                if (variable != null) {
                    return variable;
                }
            }

            return _baseContext.ResolveVariable(prefix, name);
        }

        /// <summary>
        /// When overridden in a derived class, resolves a function reference and returns an
        /// <see cref="T:System.Xml.Xsl.IXsltContextFunction"/> representing the function. The <see cref="T:System.Xml.Xsl.IXsltContextFunction"/>
        /// is used at execution time to get the return value of the function.
        /// </summary>
        /// <param name="prefix">The prefix of the function as it appears in the XPath expression.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="argTypes">An array of argument types for the function being resolved. This allows you to select between methods with the same name (for example, overloaded methods).</param>
        /// <returns>
        /// An <see cref="T:System.Xml.Xsl.IXsltContextFunction"/> representing the function.
        /// </returns>
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
        {
            if (_functionResolver != null) {
                var function = _functionResolver.ResolveFunction(prefix, name, argTypes);
                if (function != null) {
                    return function;
                }
            }

            return _baseContext.ResolveFunction(prefix, name, argTypes);
        }

        /// <summary>
        /// When overridden in a derived class, evaluates whether to preserve white space nodes or strip them for the given context.
        /// </summary>
        /// <param name="node">The white space node that is to be preserved or stripped in the current context.</param>
        /// <returns>
        /// Returns true if the white space is to be preserved or false if the white space is to be stripped.
        /// </returns>
        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return _baseContext.PreserveWhitespace(node);
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether to include white space nodes in the output.
        /// </summary>
        /// <value></value>
        /// <returns>true to check white space nodes in the source document for inclusion in the output; false to not evaluate white space nodes. The default is true.
        /// </returns>
        public override bool Whitespace
        {
            get { return _baseContext.Whitespace; }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _baseContext.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return _baseContext.Equals(obj);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return _baseContext.ToString();
        }

        /// <summary>
        /// Pushes a namespace scope onto the stack.
        /// </summary>
        public override void PushScope()
        {
            _baseContext.PushScope();
        }

        /// <summary>
        /// Pops a namespace scope off the stack.
        /// </summary>
        /// <returns>
        /// true if there are namespace scopes left on the stack; false if there are no more namespaces to pop.
        /// </returns>
        public override bool PopScope()
        {
            return _baseContext.PopScope();
        }

        /// <summary>
        /// Removes the given namespace for the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix for the namespace</param>
        /// <param name="uri">The namespace to remove for the given prefix. The namespace removed is from the current namespace scope. Namespaces outside the current scope are ignored.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The value of <paramref name="prefix"/> or <paramref name="uri"/> is null.
        /// </exception>
        public override void RemoveNamespace(string prefix, string uri)
        {
            throw new ReadOnlyException();
        }

        /// <summary>
        /// Returns an enumerator to use to iterate through the namespaces in the <see cref="T:System.Xml.XmlNamespaceManager"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> containing the prefixes stored by the <see cref="T:System.Xml.XmlNamespaceManager"/>.
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return _baseContext.GetEnumerator();
        }

        /// <summary>
        /// Gets a collection of namespace names keyed by prefix which can be used to enumerate the namespaces currently in scope.
        /// </summary>
        /// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope"/> value that specifies the type of namespace nodes to return.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Specialized.StringDictionary"/> object containing a collection of namespace and prefix pairs currently in scope.
        /// </returns>
        public override IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return _baseContext.GetNamespacesInScope(scope);
        }

        /// <summary>
        /// Gets the namespace URI for the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass String.EmptyFalse.</param>
        /// <returns>
        /// Returns the namespace URI for <paramref name="prefix"/> or null if there is no mapped namespace. The returned string is atomized.
        /// For more information on atomized strings, see <see cref="T:System.Xml.XmlNameTable"/>.
        /// </returns>
        public override string LookupNamespace(string prefix)
        {
            return _baseContext.LookupNamespace(prefix);
        }

        /// <summary>
        /// Finds the prefix declared for the given namespace URI.
        /// </summary>
        /// <param name="uri">The namespace to resolve for the prefix.</param>
        /// <returns>
        /// The matching prefix. If there is no mapped prefix, the method returns String.EmptyFalse. If a null value is supplied, then null is returned.
        /// </returns>
        public override string LookupPrefix(string uri)
        {
            return _baseContext.LookupPrefix(uri);
        }

        /// <summary>
        /// Gets a value indicating whether the supplied prefix has a namespace defined for the current pushed scope.
        /// </summary>
        /// <param name="prefix">The prefix of the namespace you want to find.</param>
        /// <returns>
        /// true if there is a namespace defined; otherwise, false.
        /// </returns>
        public override bool HasNamespace(string prefix)
        {
            return _baseContext.HasNamespace(prefix);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Xml.XmlNameTable"/> associated with this object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Xml.XmlNameTable"/> used by this object.
        /// </returns>
        public override XmlNameTable NameTable
        {
            get { return _baseContext.NameTable; }
        }
    }

}
