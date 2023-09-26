///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Getter for a DOM complex element.
    /// </summary>
    public class DOMComplexElementGetter : EventPropertyGetterSPI,
        DOMPropertyGetter
    {
        private readonly FragmentFactorySPI fragmentFactory;
        private readonly bool isArray;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        /// <param name="isArray">if this is an array property</param>
        public DOMComplexElementGetter(
            string propertyName,
            FragmentFactorySPI fragmentFactory,
            bool isArray)
        {
            this.propertyName = propertyName;
            this.fragmentFactory = fragmentFactory;
            this.isArray = isArray;
        }

        public object GetValueAsFragment(XmlNode node)
        {
            if (!isArray) {
                return GetValueAsNodeFragment(node, propertyName, fragmentFactory);
            }

            return GetValueAsNodeFragmentArray(node, propertyName, fragmentFactory);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetValueAsNode(node, propertyName);
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return GetValueAsNodeArray(node, propertyName);
        }

        public CodegenExpression GetValueAsNodeCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "GetValueAsNode", value, Constant(propertyName));
        }

        public CodegenExpression GetValueAsNodeArrayCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "GetValueAsNodeArray", value, Constant(propertyName));
        }

        public CodegenExpression GetValueAsFragmentCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(value, codegenMethodScope, codegenClassScope);
        }

        public object Get(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode underlying)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type Node");
            }

            if (!isArray) {
                return GetValueAsNode(underlying);
            }
            else {
                var node = underlying;
                return GetValueAsNodeArray(node);
            }
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode node)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type Node");
            }

            return GetValueAsFragment(node);
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
            if (!isArray) {
                return StaticMethod(GetType(), "GetValueAsNode", underlyingExpression, Constant(propertyName));
            }

            return StaticMethod(GetType(), "GetValueAsNodeArray", underlyingExpression, Constant(propertyName));
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
            var ff = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(FragmentFactory),
                fragmentFactory.Make(codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
            if (!isArray) {
                return StaticMethod(
                    GetType(),
                    "GetValueAsNodeFragment",
                    underlyingExpression,
                    Constant(propertyName),
                    ff);
            }

            return StaticMethod(
                GetType(),
                "GetValueAsNodeFragmentArray",
                underlyingExpression,
                Constant(propertyName),
                ff);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">prop name</param>
        /// <returns>node</returns>
        public static XmlNode GetValueAsNode(
            XmlNode node,
            string propertyName)
        {
            var list = node.ChildNodes;
            for (var i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (childNode.LocalName == propertyName) {
                    return childNode;
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">prop name</param>
        /// <returns>node</returns>
        public static XmlNode[] GetValueAsNodeArray(
            XmlNode node,
            string propertyName)
        {
            var list = node.ChildNodes;

            var count = 0;
            for (var i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType == XmlNodeType.Element) {
                    count++;
                }
            }

            if (count == 0) {
                return Array.Empty<XmlNode>();
            }

            var nodes = new XmlNode[count];
            var realized = 0;
            for (var i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (childNode.LocalName == propertyName) {
                    nodes[realized++] = childNode;
                }
            }

            if (realized == count) {
                return nodes;
            }

            if (realized == 0) {
                return Array.Empty<XmlNode>();
            }

            var shrunk = new XmlNode[realized];
            Array.Copy(nodes, 0, shrunk, 0, realized);
            return shrunk;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">prop name</param>
        /// <param name="fragmentFactory">fragment factory</param>
        /// <returns>node</returns>
        public static object GetValueAsNodeFragment(
            XmlNode node,
            string propertyName,
            FragmentFactory fragmentFactory)
        {
            var result = GetValueAsNode(node, propertyName);
            if (result == null) {
                return null;
            }

            return fragmentFactory.GetEvent(result);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">prop name</param>
        /// <param name="fragmentFactory">fragment factory</param>
        /// <returns>node</returns>
        public static object GetValueAsNodeFragmentArray(
            XmlNode node,
            string propertyName,
            FragmentFactory fragmentFactory)
        {
            var result = GetValueAsNodeArray(node, propertyName);
            if (result == null || result.Length == 0) {
                return Array.Empty<EventBean>();
            }

            var events = new EventBean[result.Length];
            var count = 0;
            for (var i = 0; i < result.Length; i++) {
                events[count++] = fragmentFactory.GetEvent(result[i]);
            }

            return events;
        }
    }
} // end of namespace