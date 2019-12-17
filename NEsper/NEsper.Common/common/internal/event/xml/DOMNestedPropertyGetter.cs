///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Getter for nested properties in a DOM tree.
    /// </summary>
    public class DOMNestedPropertyGetter : EventPropertyGetterSPI,
        DOMPropertyGetter
    {
        private readonly DOMPropertyGetter[] domGetterChain;
        private readonly FragmentFactorySPI fragmentFactory;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMNestedPropertyGetter(
            IList<EventPropertyGetter> getterChain,
            FragmentFactorySPI fragmentFactory)
        {
            domGetterChain = new DOMPropertyGetter[getterChain.Count];
            this.fragmentFactory = fragmentFactory;

            var count = 0;
            foreach (var getter in getterChain) {
                domGetterChain[count++] = (DOMPropertyGetter) getter;
            }
        }

        public object GetValueAsFragment(XmlNode node)
        {
            var result = GetValueAsNode(node);
            if (result == null) {
                return null;
            }

            return fragmentFactory.GetEvent(result);
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            for (var i = 0; i < domGetterChain.Length - 1; i++) {
                node = domGetterChain[i].GetValueAsNode(node);
                if (node == null) {
                    return null;
                }
            }

            return domGetterChain[domGetterChain.Length - 1].GetValueAsNodeArray(node);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            for (var i = 0; i < domGetterChain.Length; i++) {
                node = domGetterChain[i].GetValueAsNode(node);
                if (node == null) {
                    return null;
                }
            }

            return node;
        }

        public CodegenExpression GetValueAsNodeCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetValueAsNodeCodegen(codegenMethodScope, codegenClassScope), value);
        }

        public CodegenExpression GetValueAsNodeArrayCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetValueAsNodeArrayCodegen(codegenMethodScope, codegenClassScope), value);
        }

        public CodegenExpression GetValueAsFragmentCodegen(
            CodegenExpression value,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetValueAsFragmentCodegen(codegenMethodScope, codegenClassScope), value);
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

        public bool IsExistsProperty(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type XmlNode");
            }

            return IsExistsProperty((XmlNode) obj.Underlying);
        }

        public object GetFragment(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode)) {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type XmlNode");
            }

            var value = (XmlNode) obj.Underlying;

            for (var i = 0; i < domGetterChain.Length - 1; i++) {
                value = domGetterChain[i].GetValueAsNode(value);

                if (value == null) {
                    return null;
                }
            }

            return domGetterChain[domGetterChain.Length - 1].GetValueAsFragment(value);
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
            return LocalMethod(GetValueAsNodeCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetValueAsFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var member = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(FragmentFactory),
                fragmentFactory.Make(codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block
                .DeclareVar<XmlNode>(
                    "result",
                    GetValueAsNodeCodegen(Ref("node"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(member, "GetEvent", Ref("result")));
        }

        private CodegenMethod GetValueAsNodeArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(XmlNode[]), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block;
            for (var i = 0; i < domGetterChain.Length - 1; i++) {
                block.AssignRef(
                    "node",
                    domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), codegenMethodScope, codegenClassScope));
                block.IfRefNullReturnNull("node");
            }

            return block.MethodReturn(
                domGetterChain[domGetterChain.Length - 1]
                    .GetValueAsNodeArrayCodegen(
                        Ref("node"),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private CodegenMethod GetValueAsNodeCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(XmlNode), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block;
            for (var i = 0; i < domGetterChain.Length; i++) {
                block.AssignRef(
                    "node",
                    domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), codegenMethodScope, codegenClassScope));
                block.IfRefNullReturnNull("node");
            }

            return block.MethodReturn(Ref("node"));
        }

        private bool IsExistsProperty(XmlNode value)
        {
            for (var i = 0; i < domGetterChain.Length; i++) {
                value = domGetterChain[i].GetValueAsNode(value);
                if (value == null) {
                    return false;
                }
            }

            return true;
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "value")
                .Block;
            for (var i = 0; i < domGetterChain.Length; i++) {
                block.AssignRef(
                    "value",
                    domGetterChain[i].GetValueAsNodeCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
                block.IfRefNullReturnFalse("value");
            }

            return block.MethodReturn(ConstantTrue());
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "value")
                .Block;
            for (var i = 0; i < domGetterChain.Length - 1; i++) {
                block.AssignRef(
                    "value",
                    domGetterChain[i].GetValueAsNodeCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
                block.IfRefNullReturnNull("value");
            }

            return block.MethodReturn(
                domGetterChain[domGetterChain.Length - 1]
                    .UnderlyingFragmentCodegen(
                        Ref("value"),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace