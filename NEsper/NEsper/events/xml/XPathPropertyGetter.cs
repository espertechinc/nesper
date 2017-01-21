///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for properties of DOM xml events.
    /// </summary>
    public class XPathPropertyGetter : EventPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    	private readonly XPathExpression _expression;
        private readonly String _expressionText;
        private readonly String _property;
    	private readonly XPathResultType _resultType;
        private readonly SimpleTypeParser _simpleTypeParser;
        private readonly Type _optionalCastToType;
        private readonly bool _isCastToArray;
        private readonly FragmentFactory _fragmentFactory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the name of the event property for which this getter gets values</param>
        /// <param name="expressionText">is the property expression itself</param>
        /// <param name="xPathExpression">is a compile XPath expression</param>
        /// <param name="resultType">is the resulting type</param>
        /// <param name="optionalCastToType">if non-null then the return value of the xpath expression is cast to this value</param>
        /// <param name="fragmentFactory">for creating fragments, or null in none to be created</param>
        public XPathPropertyGetter(String propertyName,
                                   String expressionText,
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
    
            if ((optionalCastToType != null) && (optionalCastToType.IsArray))
            {
                _isCastToArray = true;

                if (resultType != XPathResultType.NodeSet)
                {
                    throw new ArgumentException("Array cast-to types require XPathResultType.NodeSet as the XPath result type");
                }
                optionalCastToType = optionalCastToType.GetElementType();
            }
            else
            {
                _isCastToArray = false;
            }
    
            if (optionalCastToType != null)
            {
                _simpleTypeParser = SimpleTypeParserFactory.GetParser(optionalCastToType);
            }
            else
            {
                _simpleTypeParser = null;
            }
            if (optionalCastToType == typeof(XmlNode))
            {
                _optionalCastToType = null;
            }
            else
            {
                _optionalCastToType = optionalCastToType;
            }
        }
    
    	public Object Get(EventBean eventBean)
        {
    		var und = eventBean.Underlying;
            if (und == null)
            {
                throw new PropertyAccessException("Unexpected null underlying event encountered, expecting System.Xml.XmlNode instance as underlying");
            }

    	    XPathNavigator navigator;

    	    var xnode = und as XElement;
            if (xnode == null) {
                var node = und as XmlNode;
                if (node == null) {
                    throw new PropertyAccessException("Unexpected underlying event of type '" + und.GetType().FullName +
                                                      "' encountered, expecting System.Xml.XmlNode as underlying");
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(
                        "Running XPath '{0}' for property '{1}' against Node XML : {2}",
                        _expressionText,
                        _property,
                        SchemaUtil.Serialize((XmlNode)und));
                }

                navigator = node.CreateNavigator();
            } else {
                navigator = xnode.CreateNavigator();
            }

    	    try
    	    {
                var result = navigator.Evaluate(_expression);
                if (result == null) {
                    return null;
                }

                // if there is no parser, return xpath expression type
                if (_optionalCastToType == null)
                {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator != null) {
                        if (nodeIterator.Count == 0)
                        {
                            return null;
                        }
                        if (nodeIterator.Count == 1) {
                            nodeIterator.MoveNext();
                            switch( _resultType ) {
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
                        } else {
                            return new XPathIteratorNodeList(nodeIterator);
                        }
                    }

                    return result;
                }
    
                if (_isCastToArray)
                {
                    return CastToArray(result);
                }
    
                if ( result is XPathNodeIterator )
                {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator.Count == 0) return null;
                    if (nodeIterator.Count == 1)
                    {
                        nodeIterator.MoveNext();
                        result = nodeIterator.Current.TypedValue;
                    }
                    else
                    {
                        if (_simpleTypeParser == null)
                        {
                            var resultList = new List<object>();
                            while (nodeIterator.MoveNext())
                            {
                                result = nodeIterator.Current.TypedValue;
                                resultList.Add(result);
                            }

                            return resultList.ToArray();
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                // string results get parsed
                if (result is String)
                {
                    try
                    {
                        return _simpleTypeParser.Invoke((string) result);
                    }
                    catch
                    {
                        Log.Warn("Error parsing XPath property named '" + _property + "' expression result '" + result + " as type " + _optionalCastToType.Name);
                        return null;
                    }
                }
    
                // coercion
                if (result is Double)
                {
                    try
                    {
                        return CoercerFactory.CoerceBoxed(result, _optionalCastToType);
                    }
                    catch
                    {
                        Log.Warn("Error coercing XPath property named '" + _property + "' expression result '" + result + " as type " + _optionalCastToType.Name);
                        return null;
                    }
                }
    
                // check bool type
                if (result is Boolean)
                {
                    if (_optionalCastToType != typeof(bool?))
                    {
                        Log.Warn("Error coercing XPath property named '" + _property + "' expression result '" + result + " as type " + _optionalCastToType.Name);
                        return null;
                    }
                    return result;
                }
    
                Log.Warn("Error processing XPath property named '" + _property + "' expression result '" + result + ", not a known type");
                return null;
            }
            catch (XPathException e) {
    			throw new PropertyAccessException("Error getting property " + _property,e);
    		}
    	}
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            if (_fragmentFactory == null)
            {
                return null;
            }

            Object und = eventBean.Underlying;
            if (und == null)
            {
                throw new PropertyAccessException("Unexpected null underlying event encountered, expecting System.Xml.XmlNode instance as underlying");
            }

            var node = und as XmlNode;
            if (node == null)
            {
                throw new PropertyAccessException("Unexpected underlying event of type '" + und.GetType().FullName + "' encountered, expecting System.Xml.XmlNode as underlying");
            }
    
            try {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(
                        "Running XPath '{0}' for property '{1}' against Node XML : {2}",
                        _expressionText,
                        _property,
                        SchemaUtil.Serialize((XmlNode) und));
                }

                var navigator = node.CreateNavigator();
                var result = navigator.Evaluate(_expression);
                if (result == null)
                {
                    return null;
                }

                if (result is XPathNodeIterator)
                {
                    var nodeIterator = result as XPathNodeIterator;
                    if (nodeIterator.Count == 0) return null;
                    if (nodeIterator.Count == 1)
                    {
                        nodeIterator.MoveNext();
                        return _fragmentFactory.GetEvent(((IHasXmlNode) nodeIterator.Current).GetNode());
                    }

                    var events = new List<EventBean>();
                    while (nodeIterator.MoveNext()) {
                        events.Add(_fragmentFactory.GetEvent(((IHasXmlNode) nodeIterator.Current).GetNode()));
                    }

                    return events.ToArray();
                }

                Log.Warn("Error processing XPath property named '" + _property + "' expression result is not of type Node or Nodeset");
                return null;
            }
            catch (XPathException e)
            {
                throw new PropertyAccessException("Error getting property " + _property,e);
            }
        }
    
        private Object CastToArray(XPathNodeIterator nodeIterator)
        {
            var itemList = new List<object>();
            while(nodeIterator.MoveNext()) {
                var item = nodeIterator.Current;
                if (item != null) {
                    try {
                        if ((item.NodeType == XPathNodeType.Attribute) ||
                            (item.NodeType == XPathNodeType.Element)) {
                            var textContent = item.InnerXml;
                            itemList.Add(_simpleTypeParser.Invoke(textContent));
                        }
                    }
                    catch {
                        if (Log.IsInfoEnabled) {
                            Log.Info("Parse error for text content {0} for expression {1}", item.InnerXml, _expression);
                        }
                    }
                }
            }

            var array = Array.CreateInstance(_optionalCastToType, itemList.Count);
            for( int ii = 0 ; ii < itemList.Count ; ii++ ) {
                array.SetValue(itemList[ii], ii);
            }

            return array;
        }

        private Object CastToArray(Object result)
        {
            if (result is XPathNodeIterator) {
                return CastToArray((XPathNodeIterator) result);
            }

            if (!(result is XmlNodeList))
            {
                return null;
            }

            var nodeList = (XmlNodeList)result;
            var array = Array.CreateInstance(_optionalCastToType, nodeList.Count);

            for (int i = 0; i < nodeList.Count; i++)
            {
                Object arrayItem = null;
                try
                {
                    XmlNode item = nodeList.Item(i);
                    String textContent;
                    if ((item.NodeType == XmlNodeType.Attribute) ||
                        (item.NodeType == XmlNodeType.Element)) {
                        textContent = item.InnerText;
                    }
                    else
                    {
                        continue;
                    } 
                    
                    arrayItem = _simpleTypeParser.Invoke(textContent);
                }
                catch
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info("Parse error for text content {0} for expression {1}", nodeList[i].InnerText, _expression);
                    }
                }

                array.SetValue(arrayItem, i);
            }
            
            return array;
        }
    }
}
