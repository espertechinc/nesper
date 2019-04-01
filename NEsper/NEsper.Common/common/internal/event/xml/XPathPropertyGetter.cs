///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Getter for properties of DOM xml events.
    /// </summary>
    /// <author>pablo</author>
    public class XPathPropertyGetter : EventPropertyGetterSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BaseXMLEventType baseXMLEventType;
        private readonly XPathExpression expression;
        private readonly string expressionText;
        private readonly FragmentFactory fragmentFactory;
        private readonly bool isCastToArray;
        private readonly Type optionalCastToType;
        private readonly XPathResultType resultType;
        private readonly SimpleTypeParser simpleTypeParser;

        public XPathPropertyGetter(
            BaseXMLEventType baseXMLEventType, 
            string propertyName, 
            string expressionText,
            XPathExpression xPathExpression,
            XPathResultType resultType, 
            Type optionalCastToType, 
            FragmentFactory fragmentFactory)
        {
            expression = xPathExpression;
            this.expressionText = expressionText;
            Property = propertyName;
            this.resultType = resultType;
            this.fragmentFactory = fragmentFactory;
            this.baseXMLEventType = baseXMLEventType;

            if (optionalCastToType != null && optionalCastToType.IsArray) {
                isCastToArray = true;
                if (resultType != XPathResultType.NodeSet) {
                    throw new ArgumentException(
                        "Array cast-to types require XPathConstants.NODESET as the XPath result type");
                }

                optionalCastToType = optionalCastToType.GetElementType();
            }
            else {
                isCastToArray = false;
            }

            if (optionalCastToType != null) {
                simpleTypeParser = SimpleTypeParserFactory.GetParser(optionalCastToType);
            }
            else {
                simpleTypeParser = null;
            }

            if (optionalCastToType == typeof(XmlNode)) {
                this.optionalCastToType = null;
            }
            else {
                this.optionalCastToType = optionalCastToType;
            }
        }

        public string Property { get; }

        public object Get(EventBean eventBean)
        {
            var und = eventBean.Underlying;
            if (und == null) {
                throw new PropertyAccessException(
                    "Unexpected null underlying event encountered, expecting org.w3c.dom.XmlNode instance as underlying");
            }

            if (!(und is XmlNode)) {
                throw new PropertyAccessException(
                    "Unexpected underlying event of type '" + und.GetType() +
                    "' encountered, expecting XmlNode as underlying");
            }

            return GetFromUnderlying((XmlNode) und);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean eventBean)
        {
            if (fragmentFactory == null) {
                return null;
            }

            var und = eventBean.Underlying;
            if (und == null) {
                throw new PropertyAccessException(
                    "Unexpected null underlying event encountered, expecting org.w3c.dom.XmlNode instance as underlying");
            }

            if (!(und is XmlNode)) {
                throw new PropertyAccessException(
                    "Unexpected underlying event of type '" + und.GetType() +
                    "' encountered, expecting XmlNode as underlying");
            }

            return GetFragmentFromUnderlying(und);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var xpathGetter = codegenClassScope.AddOrGetFieldSharable(
                new XPathPropertyGetterCodegenFieldSharable(baseXMLEventType, this));
            return ExprDotMethod(xpathGetter, "getFromUnderlying", Cast(typeof(XmlNode), underlyingExpression));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (fragmentFactory == null) {
                return ConstantNull();
            }

            var xpathGetter = codegenClassScope.AddOrGetFieldSharable(
                new XPathPropertyGetterCodegenFieldSharable(baseXMLEventType, this));
            return ExprDotMethod(xpathGetter, "getFragmentFromUnderlying", underlyingExpression);
        }

        public object GetFromUnderlying(XmlNode node)
        {
            return EvaluateXPathGet(
                node, expression, expressionText, Property, optionalCastToType, resultType, isCastToArray,
                simpleTypeParser);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="expression">xpath</param>
        /// <param name="expressionText">text</param>
        /// <param name="property">prop</param>
        /// <param name="optionalCastToType">type or null</param>
        /// <param name="resultType">result xpath type</param>
        /// <param name="isCastToArray">array indicator</param>
        /// <param name="simpleTypeParser">parse</param>
        /// <returns>value</returns>
        public static object EvaluateXPathGet(
            XmlNode und, 
            XPathExpression expression, 
            string expressionText, 
            string property, 
            Type optionalCastToType,
            XPathResultType resultType, 
            bool isCastToArray, 
            SimpleTypeParser simpleTypeParser)
        {
            try {
                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Running XPath '" + expressionText + "' for property '" + property + "' against XmlNode XML :" +
                        SchemaUtil.Serialize(und));
                }

                // if there is no parser, return xpath expression type
                if (optionalCastToType == null) {
                    return expression.Evaluate(und, resultType);
                }

                // obtain result
                object result = expression.Evaluate(und, resultType);
                if (result == null) {
                    return null;
                }

                if (isCastToArray) {
                    return CastToArray(result, optionalCastToType, simpleTypeParser, expression);
                }

                // string results get parsed
                if (result is string) {
                    try {
                        return simpleTypeParser.Parse(result.ToString());
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        Log.Warn(
                            "Error parsing XPath property named '" + property + "' expression result '" + result +
                            " as type " + optionalCastToType.Name);
                        return null;
                    }
                }

                // coercion
                if (result is double) {
                    try {
                        return TypeHelper.CoerceBoxed(result, optionalCastToType);
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        Log.Warn(
                            "Error coercing XPath property named '" + property + "' expression result '" + result +
                            " as type " + optionalCastToType.Name);
                        return null;
                    }
                }

                // check boolean type
                if (result is bool) {
                    if (optionalCastToType != typeof(bool?)) {
                        Log.Warn(
                            "Error coercing XPath property named '" + property + "' expression result '" + result +
                            " as type " + optionalCastToType.Name);
                        return null;
                    }

                    return result;
                }

                Log.Warn(
                    "Error processing XPath property named '" + property + "' expression result '" + result +
                    ", not a known type");
                return null;
            }
            catch (XPathExpressionException e) {
                throw new PropertyAccessException("Error getting property " + property, e);
            }
        }

        public static object EvaluateXPathFragment(
            object und,
            XPathExpression expression, 
            string expressionText, 
            string property,
            FragmentFactory fragmentFactory,
            XPathResultType resultType)
        {
            try {
                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Running XPath '" + expressionText + "' for property '" + property + "' against XmlNode XML :" +
                        SchemaUtil.Serialize((XmlNode) und));
                }

                object result = expression.Evaluate(und, resultType);

                if (result is XmlNode) {
                    return fragmentFactory.GetEvent((XmlNode) result);
                }

                if (result is XmlNodeList) {
                    var nodeList = (XmlNodeList) result;
                    var events = new EventBean[nodeList.Count];
                    for (var i = 0; i < events.Length; i++) {
                        events[i] = fragmentFactory.GetEvent(nodeList.Item(i));
                    }

                    return events;
                }

                Log.Warn(
                    "Error processing XPath property named '" + property +
                    "' expression result is not of type XmlNode or Nodeset");
                return null;
            }
            catch (XPathExpressionException e) {
                throw new PropertyAccessException("Error getting property " + property, e);
            }
        }

        public object GetFragmentFromUnderlying(object und)
        {
            return EvaluateXPathFragment(und, expression, expressionText, Property, fragmentFactory, resultType);
        }

        private static object CastToArray(
            object result, Type optionalCastToType, SimpleTypeParser simpleTypeParser, XPathExpression expression)
        {
            if (!(result is XmlNodeList)) {
                return null;
            }

            var nodeList = (XmlNodeList) result;
            var array = Array.CreateInstance(optionalCastToType, nodeList.Count);

            for (var i = 0; i < nodeList.Count; i++) {
                object arrayItem = null;
                try {
                    var item = nodeList.Item(i);
                    string textContent;
                    if (item.NodeType == XmlNodeType.Attribute || item.NodeType == XmlNodeType.Element) {
                        textContent = nodeList.Item(i).InnerText;
                    }
                    else {
                        continue;
                    }

                    arrayItem = simpleTypeParser.Parse(textContent);
                }
                catch (Exception ex) {
                    if (Log.IsInfoEnabled) {
                        Log.Info(
                            "Parse error for text content " + nodeList.Item(i).InnerText + " for expression " +
                            expression);
                    }
                }

                array.SetValue(arrayItem, i);
            }

            return array;
        }
    }
} // end of namespace