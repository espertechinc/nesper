///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    ///     Annotation for use with XML schemas. Only the root element name is required.
    /// </summary>
    public class XMLSchemaAttribute : Attribute
    {
        /// <summary>
        ///     The root element name (required).
        /// </summary>
        /// <returns>root element name</returns>
        public virtual string RootElementName { get; set; }

        /// <summary>
        ///     The schema resource URL
        /// </summary>
        /// <returns>url</returns>
        public virtual string SchemaResource { get; set; } = "";

        /// <summary>
        ///     The schema text
        /// </summary>
        /// <returns>schema</returns>
        public virtual string SchemaText { get; set; } = "";

        /// <summary>
        ///     Set to false (the default) to indicate that property expressions are evaluated by the DOM-walker
        ///     implementation (the default), or set to true to indicate that property expressions are rewritten into XPath expressions.
        /// </summary>
        /// <returns>xpath property use</returns>
        public virtual bool XPathPropertyExpr { get; set; } = false;

        /// <summary>
        ///     When set to true (the default), indicates that when properties are compiled to XPath expressions that the
        ///     compilation should generate an absolute XPath expression such as "/getQuote/request" for the
        ///     simple request property, or "/getQuote/request/symbol" for a "request.symbol" nested property,
        ///     wherein the root element node is "getQuote".
        ///     <para>
        ///         When set to false, indicates that when properties are compiled to XPath expressions that the
        ///         compilation should generate a deep XPath expression such as "//symbol" for the
        ///         simple symbol property, or "//request/symbol" for a "request.symbol" nested property.
        ///     </para>
        /// </summary>
        /// <returns>xpath resolve properties absolute flag</returns>
        public virtual bool XPathResolvePropertiesAbsolute { get; set; } = true;

        /// <summary>
        ///     The default namespace
        /// </summary>
        /// <returns>default namespace</returns>
        public virtual string DefaultNamespace { get; set; } = "";

        /// <summary>
        ///     The root element namespace
        /// </summary>
        /// <returns>root element namespace</returns>
        public virtual string RootElementNamespace { get; set; } = "";

        /// <summary>
        ///     Set to true (the default) to indicate that an <seealso cref="EventSender" /> returned for this event type validates
        ///     the root document element name against the one configured (the default), or false to not validate the root document
        ///     element name as configured.
        /// </summary>
        /// <returns>flag</returns>
        public virtual bool EventSenderValidatesRoot { get; set; } = true;

        /// <summary>
        ///     Set to true (the default) to look up or create event types representing fragments of an XML document
        ///     automatically upon request for fragment event type information; Or false when only explicit
        ///     properties may return fragments.
        /// </summary>
        /// <returns>flag</returns>
        public virtual bool AutoFragment { get; set; } = true;

        /// <summary>
        ///     Sets the class name of the XPath function resolver to be assigned to the XPath factory instance
        ///     upon type initialization.
        /// </summary>
        /// <returns>class name</returns>
        public virtual string XPathFunctionResolver { get; set; } = "";

        /// <summary>
        ///     Sets the class name of the XPath variable resolver to be assigned to the XPath factory instance
        ///     upon type initialization.
        /// </summary>
        /// <returns>class name</returns>
        public virtual string XPathVariableResolver { get; set; } = "";
    }
} // end of namespace