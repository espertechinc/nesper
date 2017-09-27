///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.parse;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Parses event property names and transforms to XPath expressions. Supports
    /// nested, indexed and mapped event properties.
    /// </summary>

    public class SimpleXMLPropertyParser
    {
        /// <summary>
        /// Return the xPath corresponding to the given property. The PropertyName String
        /// may be simple, nested, indexed or mapped.
        /// </summary>
        /// <param name="ast">is the property tree AST</param>
        /// <param name="propertyName">is the property name to parse</param>
        /// <param name="rootElementName">is the name of the root element for generating the XPath expression</param>
        /// <param name="defaultNamespacePrefix">is the prefix of the default namespace</param>
        /// <param name="isResolvePropertiesAbsolute">is true to indicate to resolve XPath properties as absolute propsor relative props </param>
        /// <returns>
        /// xpath expression
        /// </returns>
        public static String Walk(EsperEPL2GrammarParser.StartEventPropertyRuleContext ast, String propertyName, String rootElementName, String defaultNamespacePrefix, bool isResolvePropertiesAbsolute)
        {
            var xPathBuf = new StringBuilder();
            xPathBuf.Append('/');
            if (isResolvePropertiesAbsolute)
            {
                if (defaultNamespacePrefix != null)
                {
                    xPathBuf.Append(defaultNamespacePrefix);
                    xPathBuf.Append(':');
                }
                xPathBuf.Append(rootElementName);
            }

            IList<EsperEPL2GrammarParser.EventPropertyAtomicContext> ctxs = ast.eventProperty().eventPropertyAtomic();
            if (ctxs.Count == 1)
            {
                xPathBuf.Append(MakeProperty(ctxs[0], defaultNamespacePrefix));
            }
            else
            {
                foreach (EsperEPL2GrammarParser.EventPropertyAtomicContext ctx in ctxs)
                {
                    xPathBuf.Append(MakeProperty(ctx, defaultNamespacePrefix));
                }
            }

            String xPath = xPathBuf.ToString();

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".parse For property '" + propertyName + "' the xpath is '" + xPath + "'");
            }

            return xPath;
            //return XPathExpression.Compile( xPath );
        }

        private static String MakeProperty(EsperEPL2GrammarParser.EventPropertyAtomicContext ctx, String defaultNamespacePrefix)
        {
            String prefix = "";
            if (defaultNamespacePrefix != null)
            {
                prefix = defaultNamespacePrefix + ":";
            }

            String unescapedIdent = ASTUtil.UnescapeDot(ctx.eventPropertyIdent().GetText());
            if (ctx.lb != null)
            {
                int index = IntValue.ParseString(ctx.number().GetText());
                int xPathPosition = index + 1;
                return '/' + prefix + unescapedIdent + "[position() = " + xPathPosition + ']';
            }

            if (ctx.lp != null)
            {
                String key = StringValue.ParseString(ctx.s.Text);
                return '/' + prefix + unescapedIdent + "[@id='" + key + "']";
            }

            return '/' + prefix + unescapedIdent;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
