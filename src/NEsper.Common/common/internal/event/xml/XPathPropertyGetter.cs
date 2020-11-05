///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.xml;

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

        private readonly BaseXMLEventType _baseXMLEventType;
        private readonly XPathExpression _expression;
        private readonly string _expressionText;
        private readonly string _property;
        private readonly XPathResultType _resultType;
        private readonly SimpleTypeParser _simpleTypeParser;
        private readonly Type _optionalCastToType;
        private readonly bool _isCastToArray;
        private readonly FragmentFactory _fragmentFactory;

        public XPathPropertyGetter(
            BaseXMLEventType baseXMLEventType,
            string propertyName,
            string expressionText,
            XPathExpression xPathExpression,
            XPathResultType resultType,
            Type optionalCastToType,
            FragmentFactory fragmentFactory)
        {
            _expression = xPathExpression;
            _expressionText = expressionText;
            _property = propertyName;
            _resultType = resultType;
            _fragmentFactory = fragmentFactory;
            _baseXMLEventType = baseXMLEventType;

            if (optionalCastToType != null && optionalCastToType.IsArray) {
                _isCastToArray = true;
                if (resultType != XPathResultType.NodeSet) {
                    throw new ArgumentException(
                        "Array cast-to types require XPathResultType.NodeSet as the XPath result type");
                }

                optionalCastToType = optionalCastToType.GetElementType();
            }
            else {
                _isCastToArray = false;
            }

            if (optionalCastToType != null) {
                _simpleTypeParser = SimpleTypeParserFactory.GetParser(optionalCastToType);
            }
            else {
                _simpleTypeParser = null;
            }

            if (optionalCastToType == typeof(XmlNode)) {
                this._optionalCastToType = null;
            }
            else {
                this._optionalCastToType = optionalCastToType;
            }
        }

        public string Property => _property;

        public object Get(EventBean eventBean)
        {
            var und = eventBean.Underlying;
            if (und == null) {
                throw new PropertyAccessException(
                    "Unexpected null underlying event encountered, expecting org.w3c.dom.XmlNode instance as underlying");
            }

            var xnode = und as XElement;
            if (xnode == null) {
                var node = und as XmlNode;
                if (node == null) {
                    throw new PropertyAccessException(
                        "Unexpected underlying event of type '" +
                        und.GetType().FullName +
                        "' encountered, expecting System.Xml.XmlNode as underlying");
                }

                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Running XPath '{0}' for property '{1}' against Node XML : {2}",
                        _expressionText,
                        _property,
                        SchemaUtil.Serialize((XmlNode) und));
                }

                return GetFromUnderlying(node.CreateNavigator());
            }

            return GetFromUnderlying(xnode.CreateNavigator());
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentFactory == null) {
                return null;
            }

            var und = eventBean.Underlying;
            if (und == null) {
                throw new PropertyAccessException(
                    "Unexpected null underlying event encountered, expecting org.w3c.dom.XmlNode instance as underlying");
            }

            if (und is XmlNode node) {
                return GetFragmentFromUnderlying(node.CreateNavigator());
            }

            if (und is XContainer container) {
                return GetFragmentFromUnderlying(container.CreateNavigator());
            }

            throw new PropertyAccessException(
                "Unexpected underlying event of type '" +
                und.GetType() +
                "' encountered, expecting org.w3c.dom.Node as underlying");
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var xpathGetter = codegenClassScope.AddOrGetDefaultFieldSharable(
                new XPathPropertyGetterCodegenFieldSharable(_baseXMLEventType, this));
            return ExprDotMethod(xpathGetter, "GetFromUnderlying", Cast(typeof(XmlNode), underlyingExpression));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_fragmentFactory == null) {
                return ConstantNull();
            }

            var xpathGetter = codegenClassScope.AddOrGetDefaultFieldSharable(
                new XPathPropertyGetterCodegenFieldSharable(_baseXMLEventType, this));
            return ExprDotMethod(xpathGetter, "GetFragmentFromUnderlying", underlyingExpression);
        }

        public object GetFromUnderlying(XmlNode node)
        {
            return GetFromUnderlying(node.CreateNavigator());
        }

        public object GetFromUnderlying(XPathNavigator navigator)
        {
            return EvaluateXPathGet(
                navigator,
                _expression,
                _expressionText,
                Property,
                _optionalCastToType,
                _resultType,
                _isCastToArray,
                _simpleTypeParser);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="navigator">the xpath navigator</param>
        /// <param name="expression">xpath</param>
        /// <param name="expressionText">text</param>
        /// <param name="property">prop</param>
        /// <param name="optionalCastToType">type or null</param>
        /// <param name="resultType">result xpath type</param>
        /// <param name="isCastToArray">array indicator</param>
        /// <param name="simpleTypeParser">parse</param>
        /// <returns>value</returns>
        public static object EvaluateXPathGet(
            XPathNavigator navigator,
            XPathExpression expression,
            string expressionText,
            string property,
            Type optionalCastToType,
            XPathResultType resultType,
            bool isCastToArray,
            SimpleTypeParser simpleTypeParser)
        {
            try {
                var result = navigator.Evaluate(expression);
                if (result == null) {
                    return null;
                }

                // if there is no parser, return xpath expression type
                if (optionalCastToType == null) {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator != null) {
                        if (nodeIterator.Count == 0) {
                            return null;
                        }

                        if (nodeIterator.Count == 1) {
                            nodeIterator.MoveNext();
                            switch (resultType) {
                                case XPathResultType.Any:
                                    return ((System.Xml.IHasXmlNode) nodeIterator.Current).GetNode();

                                case XPathResultType.String:
                                    return nodeIterator.Current.TypedValue;

                                case XPathResultType.Boolean:
                                    return nodeIterator.Current.ValueAsBoolean;

                                case XPathResultType.Number:
                                    return nodeIterator.Current.ValueAsDouble;

                                default:
                                    return nodeIterator.Current.TypedValue;
                            }
                        }
                        else {
                            return new XPathIteratorNodeList(nodeIterator);
                        }
                    }

                    return result;
                }

                if (isCastToArray) {
                    return CastToArray(result, optionalCastToType, simpleTypeParser, expression);
                }

                if (result is XPathNodeIterator) {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator.Count == 0)
                        return null;
                    if (nodeIterator.Count == 1) {
                        nodeIterator.MoveNext();
                        result = nodeIterator.Current.TypedValue;
                    }
                    else {
                        if (simpleTypeParser == null) {
                            var resultList = new List<object>();
                            while (nodeIterator.MoveNext()) {
                                result = nodeIterator.Current.TypedValue;
                                resultList.Add(result);
                            }

                            return resultList.ToArray();
                        }
                        else {
                            throw new NotImplementedException();
                        }
                    }
                }

                // string results get parsed
                if (result is string) {
                    try {
                        return simpleTypeParser.Parse((string) result);
                    }
                    catch {
                        Log.Warn(
                            "Error parsing XPath property named '" +
                            property +
                            "' expression result '" +
                            result +
                            " as type " +
                            optionalCastToType.Name);
                        return null;
                    }
                }

                // coercion
                if (result is double) {
                    try {
                        return TypeHelper.CoerceBoxed(result, optionalCastToType);
                    }
                    catch {
                        Log.Warn(
                            "Error coercing XPath property named '" +
                            property +
                            "' expression result '" +
                            result +
                            " as type " +
                            optionalCastToType.Name);
                        return null;
                    }
                }

                // check bool type
                if (result is bool) {
                    if (optionalCastToType.GetBoxedType() != typeof(bool?)) {
                        Log.Warn(
                            "Error coercing XPath property named '" +
                            property +
                            "' expression result '" +
                            result +
                            " as type " +
                            optionalCastToType.Name);
                        return null;
                    }

                    return result;
                }

                Log.Warn(
                    "Error processing XPath property named '" +
                    property +
                    "' expression result '" +
                    result +
                    ", not a known type");
                return null;
            }
            catch (XPathException e) {
                throw new PropertyAccessException("Error getting property " + property, e);
            }
        }

        public static object EvaluateXPathFragment(
            XPathNavigator navigator,
            XPathExpression expression,
            string expressionText,
            string property,
            FragmentFactory fragmentFactory,
            XPathResultType resultType)
        {
            try {
                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Running XPath '{0}' for property '{1}' against Node XML : {2}",
                        expressionText,
                        property,
                        navigator);
                }

                var result = navigator.Evaluate(expression);
                if (result == null) {
                    return null;
                }

                if (result is XPathNodeIterator) {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator.Count == 0)
                        return null;
                    if (nodeIterator.Count == 1) {
                        nodeIterator.MoveNext();
                        return fragmentFactory.GetEvent(((IHasXmlNode) nodeIterator.Current).GetNode());
                    }

                    var events = new List<EventBean>();
                    while (nodeIterator.MoveNext()) {
                        events.Add(fragmentFactory.GetEvent(((IHasXmlNode) nodeIterator.Current).GetNode()));
                    }

                    return events.ToArray();
                }

                Log.Warn(
                    "Error processing XPath property named '" +
                    property +
                    "' expression result is not of type Node or Nodeset");
                return null;
            }
            catch (XPathException e) {
                throw new PropertyAccessException("Error getting property " + property, e);
            }
        }

        public object GetFragmentFromUnderlying(XPathNavigator navigator)
        {
            return EvaluateXPathFragment(
                navigator,
                _expression,
                _expressionText,
                Property,
                _fragmentFactory,
                _resultType);
        }

        public object GetFragmentFromUnderlying(XmlNode node)
        {
            return GetFragmentFromUnderlying(node.CreateNavigator());
        }

        private static object CastToArray(
            object result,
            Type optionalCastToType,
            SimpleTypeParser simpleTypeParser,
            XPathExpression expression)
        {
            if (result is XPathNodeIterator nodeIterator) {
                if (nodeIterator.Count == 0) {
                    return null;
                }

                var array = Arrays.CreateInstanceChecked(optionalCastToType, nodeIterator.Count);
                for (var i = 0; nodeIterator.MoveNext(); i++) {
                    var nodeCurrent = nodeIterator.Current;
                    object arrayItem = null;
                    
                    try {
                        if ((nodeCurrent.NodeType == XPathNodeType.Attribute) ||
                            (nodeCurrent.NodeType == XPathNodeType.Element)) {
                            var textContent = nodeCurrent.Value;
                            arrayItem = simpleTypeParser.Parse(textContent);
                        }
                    }
                    catch (Exception) {
                        if (Log.IsInfoEnabled) {
                            Log.Info(
                                "Parse error for text content " +
                                nodeCurrent.InnerXml +
                                " for expression " +
                                expression);
                        }
                    }
                    
                    array.SetValue(arrayItem, i);
                }

                return array;
            }
            if (result is XmlNodeList) {
                var nodeList = (XmlNodeList) result;
                var array = Arrays.CreateInstanceChecked(optionalCastToType, nodeList.Count);

                for (var i = 0; i < nodeList.Count; i++) {
                    object arrayItem = null;
                    try {
                        var item = nodeList.Item(i);
                        string textContent;
                        if (item.NodeType == XmlNodeType.Attribute || item.NodeType == XmlNodeType.Element) {
                            textContent = item.InnerText;
                        }
                        else {
                            continue;
                        }

                        arrayItem = simpleTypeParser.Parse(textContent);
                    }
                    catch (Exception) {
                        if (Log.IsInfoEnabled) {
                            Log.Info(
                                "Parse error for text content " +
                                nodeList.Item(i).InnerText +
                                " for expression " +
                                expression);
                        }
                    }

                    array.SetValue(arrayItem, i);
                }

                return array;
            }

            return null;
        }
    }
} // end of namespace