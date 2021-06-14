///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml.XPath;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    public partial class ConfigurationCommonEventTypeXMLDOM
    {
        /// <summary>
        ///     Descriptor class for event properties that are resolved via XPath-expression.
        /// </summary>
        [Serializable]
        public class XPathPropertyDesc
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="xPath">is an arbitrary XPath expression</param>
            /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
            public XPathPropertyDesc(
                string name,
                string xPath,
                XPathResultType type)
            {
                Name = name;
                XPath = xPath;
                Type = type;
            }

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="xPath">is an arbitrary XPath expression</param>
            /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
            /// <param name="optionalCastToType">if non-null then the return value of the xpath expression is cast to this value</param>
            public XPathPropertyDesc(
                string name,
                string xPath,
                XPathResultType type,
                Type optionalCastToType)
            {
                Name = name;
                XPath = xPath;
                Type = type;
                OptionalCastToType = optionalCastToType;
            }

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">is the event property name</param>
            /// <param name="xPath">is an arbitrary XPath expression</param>
            /// <param name="type">a constant obtained from System.Xml.XPath.XPathResultType.</param>
            /// <param name="eventTypeName">the name of an event type that represents the fragmented property value</param>
            public XPathPropertyDesc(
                string name,
                string xPath,
                XPathResultType type,
                string eventTypeName)
            {
                Name = name;
                XPath = xPath;
                Type = type;
                OptionalEventTypeName = eventTypeName;
            }

            public XPathPropertyDesc()
            {
            }

            /// <summary>
            ///     Returns the event property name.
            /// </summary>
            /// <returns>event property name</returns>
            public string Name { get; set; }

            /// <summary>
            ///     Returns the XPath expression.
            /// </summary>
            /// <returns>XPath expression</returns>
            public string XPath { get; set; }

            /// <summary>
            ///     Returns the XPathResultType representing the event property type.
            /// </summary>
            /// <returns>type information</returns>
            public XPathResultType Type { get; set; }

            /// <summary>
            ///     Returns the class that the return value of the xpath expression is cast to, or null if no casting.
            /// </summary>
            /// <returns>class to cast result of xpath expression to</returns>
            public Type OptionalCastToType { get; set; }

            /// <summary>
            ///     Returns the event type name assigned to the explicit property.
            /// </summary>
            /// <returns>type name</returns>
            public string OptionalEventTypeName { get; set; }
            
            public CodegenExpression ToCodegenExpression(
                CodegenMethodScope parent,
                CodegenClassScope scope)
            {
                var typeExpr = CodegenExpressionBuilder.EnumValue(typeof(XPathResultType), Type.GetName());
                
                return new CodegenSetterBuilder(typeof(XPathPropertyDesc), typeof(XPathPropertyDesc), "desc", parent, scope)
                    .Constant("Name", Name)
                    .Expression("Type", typeExpr)
                    .Constant("XPath", XPath)
                    .Constant("OptionalEventTypeName", OptionalEventTypeName)
                    .Constant("OptionalCastToType", OptionalCastToType)
                    .Build();
            }
        }
    }
}