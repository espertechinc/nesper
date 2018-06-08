///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;
using System.Xml.Xsl;
using com.espertech.esper.compat.xml;

namespace com.espertech.esper.supportregression.xml
{
    public class SupportXPathVariableResolver : IXPathVariableResolver
    {
        public IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return new Variable();
        }

        public class Variable : IXsltContextVariable
        {
            /// <summary>
            /// Evaluates the variable at runtime and returns an object that represents the value of the variable.
            /// </summary>
            /// <param name="xsltContext">An <see cref="T:System.Xml.Xsl.XsltContext"/> representing the execution context of the variable.</param>
            /// <returns>
            /// An <see cref="T:System.Object"/> representing the value of the variable. Possible return types include number, string, Boolean, document fragment, or node set.
            /// </returns>
            public object Evaluate(XsltContext xsltContext)
            {
                return "value";
            }

            /// <summary>
            /// Gets a value indicating whether the variable is local.
            /// </summary>
            /// <value></value>
            /// <returns>true if the variable is a local variable in the current context; otherwise, false.
            /// </returns>
            public bool IsLocal
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the variable is an Extensible Stylesheet Language Transformations (XSLT)
            /// parameter. This can be a parameter to a style sheet or a template.
            /// </summary>
            /// <value></value>
            /// <returns>true if the variable is an XSLT parameter; otherwise, false.
            /// </returns>
            public bool IsParam
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the <see cref="T:System.Xml.XPath.XPathResultType"/> representing the XML Path Language (XPath) type of the variable.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The <see cref="T:System.Xml.XPath.XPathResultType"/> representing the XPath type of the variable.
            /// </returns>
            public XPathResultType VariableType
            {
                get { return XPathResultType.String; }
            }
        }
    }
}
