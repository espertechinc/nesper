///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Getter for both attribute and element values, attributes are checked first.
    /// </summary>
    public class DOMAttributeAndElementGetter : EventPropertyGetterSPI,
        DOMPropertyGetter
    {
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        public DOMAttributeAndElementGetter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public object GetValueAsFragment(XmlNode node)
        {
            return null;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetNodePropertyValue(node, propertyName);
        }

        public object Get(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type XmlNode");
            }

            var node = (XmlNode) obj.Underlying;
            return GetValueAsNode(node);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            // The underlying is expected to be a map
            if (!(eventBean.Underlying is XmlNode)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type XmlNode");
            }

            var node = (XmlNode) eventBean.Underlying;
            return GetNodePropertyExists(node, propertyName);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null; // Never a fragment
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
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "getNodePropertyValue", underlyingExpression, Constant(propertyName));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "getNodePropertyExists", underlyingExpression, Constant(propertyName));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static XmlNode GetNodePropertyValue(XmlNode node, string propertyName)
        {
            var namedNodeMap = node.Attributes;
            if (namedNodeMap != null) {
                for (var i = 0; i < namedNodeMap.Count; i++) {
                    var attrNode = namedNodeMap.Item(i);
                    if (attrNode.LocalName != null) {
                        if (propertyName.Equals(attrNode.LocalName)) {
                            return attrNode;
                        }

                        continue;
                    }

                    if (propertyName.Equals(attrNode.Name)) {
                        return attrNode;
                    }
                }
            }

            var list = node.ChildNodes;
            for (var i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (childNode.LocalName != null) {
                    if (propertyName.Equals(childNode.LocalName)) {
                        return childNode;
                    }

                    continue;
                }

                if (childNode.Name.Equals(propertyName)) {
                    return childNode;
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static bool GetNodePropertyExists(XmlNode node, string propertyName)
        {
            var namedNodeMap = node.Attributes;
            if (namedNodeMap != null) {
                for (var i = 0; i < namedNodeMap.Count; i++) {
                    var attrNode = namedNodeMap.Item(i);
                    if (attrNode.LocalName != null) {
                        if (propertyName.Equals(attrNode.LocalName)) {
                            return true;
                        }

                        continue;
                    }

                    if (propertyName.Equals(attrNode.Name)) {
                        return true;
                    }
                }
            }

            var list = node.ChildNodes;
            for (var i = 0; i < list.Length; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (childNode.LocalName != null) {
                    if (propertyName.Equals(childNode.LocalName)) {
                        return true;
                    }

                    continue;
                }

                if (childNode.Name.Equals(propertyName)) {
                    return true;
                }
            }

            return false;
        }

        public CodegenExpression GetValueAsNodeCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(value, codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression GetValueAsNodeArrayCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression GetValueAsFragmentCodegen(
            CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace