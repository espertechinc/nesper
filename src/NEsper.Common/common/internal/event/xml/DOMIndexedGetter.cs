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
    ///     Getter for retrieving a value at a certain index.
    /// </summary>
    public class DOMIndexedGetter : EventPropertyGetterSPI,
        DOMPropertyGetter
    {
        private readonly FragmentFactoryDOMGetter _fragmentFactory;
        private readonly int _index;
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">index</param>
        /// <param name="fragmentFactory">for creating fragments if required</param>
        public DOMIndexedGetter(
            string propertyName,
            int index,
            FragmentFactoryDOMGetter fragmentFactory)
        {
            this._propertyName = propertyName;
            this._index = index;
            this._fragmentFactory = fragmentFactory;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public object GetValueAsFragment(XmlNode node)
        {
            if (_fragmentFactory == null) {
                return null;
            }

            var result = GetValueAsNode(node);
            if (result == null) {
                return null;
            }

            return _fragmentFactory.GetEvent(result);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetNodeValue(node, _propertyName, _index);
        }

        public CodegenExpression GetValueAsNodeCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(value, codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression GetValueAsNodeArrayCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression GetValueAsFragmentCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(value, codegenMethodScope, codegenClassScope);
        }

        public object Get(EventBean eventBean)
        {
            var result = eventBean.Underlying;
            if (!(result is XmlNode)) {
                return null;
            }

            var node = (XmlNode) result;
            return GetValueAsNode(node);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var result = eventBean.Underlying;
            if (!(result is XmlNode)) {
                return false;
            }

            var node = (XmlNode) result;
            return GetValueAsNode(node) != null;
        }

        public object GetFragment(EventBean eventBean)
        {
            var result = eventBean.Underlying;
            if (!(result is XmlNode)) {
                return null;
            }

            var node = (XmlNode) result;
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
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(XmlNode), beanExpression),
                codegenMethodScope,
                codegenClassScope);
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
            return StaticMethod(
                GetType(),
                "GetNodeValue",
                underlyingExpression,
                Constant(_propertyName),
                Constant(_index));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "GetNodeValueExists",
                underlyingExpression,
                Constant(_propertyName),
                Constant(_index));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_fragmentFactory == null) {
                return ConstantNull();
            }

            return LocalMethod(GetValueAsFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetValueAsFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var member = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(FragmentFactory),
                _fragmentFactory.Make(codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
            var method = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node");
            method.Block
                .DeclareVar<XmlNode>(
                    "result",
                    StaticMethod(
                        typeof(DOMIndexedGetter),
                        "GetNodeValue",
                        Ref("node"),
                        Constant(_propertyName),
                        Constant(_index)))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(member, "GetEvent", Ref("result")));
            return method;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        public static XmlNode GetNodeValue(
            XmlNode node,
            string propertyName,
            int index)
        {
            var list = node.ChildNodes;
            var count = 0;
            for (var i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                var elementName = childNode.LocalName;
                if (elementName != propertyName) {
                    continue;
                }

                if (count == index) {
                    return childNode;
                }

                count++;
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        public static bool GetNodeValueExists(
            XmlNode node,
            string propertyName,
            int index)
        {
            return GetNodeValue(node, propertyName, index) != null;
        }
    }
} // end of namespace