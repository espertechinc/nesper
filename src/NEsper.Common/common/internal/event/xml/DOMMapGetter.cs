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
    ///     DOM getter for Map-property.
    /// </summary>
    public class DOMMapGetter : EventPropertyGetterSPI,
        DOMPropertyGetter
    {
        private readonly FragmentFactorySPI _fragmentFactory;
        private readonly string _mapKey;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="mapKey">key in map</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMMapGetter(
            string propertyName,
            string mapKey,
            FragmentFactorySPI fragmentFactory)
        {
            _propertyMap = propertyName;
            _mapKey = mapKey;
            _fragmentFactory = fragmentFactory;
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
            return GetNodeValue(node, _propertyMap, _mapKey);
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
            return LocalMethod(GetValueAsFragmentCodegen(codegenMethodScope, codegenClassScope), value);
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
            return GetNodeValueExists(node, _propertyMap, _mapKey);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
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
            return ConstantNull();
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
                Constant(_propertyMap),
                Constant(_mapKey));
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
                Constant(_propertyMap),
                Constant(_mapKey));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyMap">property</param>
        /// <param name="mapKey">key</param>
        /// <returns>value</returns>
        public static XmlNode GetNodeValue(
            XmlNode node,
            string propertyMap,
            string mapKey)
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
                
                var elementName = childNode.LocalName;
                if (elementName != propertyMap) {
                    continue;
                }

                var attribute = childNode.Attributes?.GetNamedItem("id");
                if (attribute == null) {
                    attribute = childNode.Attributes?.GetNamedItem("Id");
                }
                
                if (attribute != null && attribute.InnerText == mapKey) {
                    return childNode;
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyMap">property</param>
        /// <param name="mapKey">key</param>
        /// <returns>exists flag</returns>
        public static bool GetNodeValueExists(
            XmlNode node,
            string propertyMap,
            string mapKey)
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
                
                var elementName = childNode.LocalName;
                if (elementName != propertyMap) {
                    continue;
                }

                var attribute = childNode.Attributes?.GetNamedItem("id");
                if (attribute == null) {
                    attribute = childNode.Attributes?.GetNamedItem("Id");
                }

                if (attribute != null && attribute.InnerText == mapKey) {
                    return true;
                }
            }

            return false;
        }

        private CodegenMethod GetValueAsFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var member = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(FragmentFactory),
                _fragmentFactory.Make(codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block
                .DeclareVar<XmlNode>(
                    "result",
                    GetValueAsNodeCodegen(Ref("node"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(member, "GetEvent", Ref("result")));
        }
    }
} // end of namespace