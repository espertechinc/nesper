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
    ///     Getter for XPath explicit properties returning an element in an array.
    /// </summary>
    public class XPathPropertyArrayItemGetter : EventPropertyGetterSPI
    {
        private readonly FragmentFactorySPI fragmentFactory;
        private readonly EventPropertyGetterSPI getter;
        private readonly int index;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">property getter returning the parent node</param>
        /// <param name="index">to get item at</param>
        /// <param name="fragmentFactory">for creating fragments, or null if not creating fragments</param>
        public XPathPropertyArrayItemGetter(
            EventPropertyGetterSPI getter,
            int index,
            FragmentFactorySPI fragmentFactory)
        {
            this.getter = getter;
            this.index = index;
            this.fragmentFactory = fragmentFactory;
        }

        public object Get(EventBean eventBean)
        {
            return GetXPathNodeListWCheck(getter.Get(eventBean), index);
        }

        public object GetFragment(EventBean eventBean)
        {
            if (fragmentFactory == null) {
                return null;
            }

            var result = (XmlNode) Get(eventBean);
            if (result == null) {
                return null;
            }

            return fragmentFactory.GetEvent(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
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
            if (fragmentFactory == null) {
                return ConstantNull();
            }

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
            return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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
            if (fragmentFactory == null) {
                return ConstantNull();
            }

            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">object</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        public static object GetXPathNodeListWCheck(
            object @object,
            int index)
        {
            if (!(@object is XmlNodeList)) {
                return null;
            }

            var nodeList = (XmlNodeList) @object;
            if (nodeList.Count <= index) {
                return null;
            }

            return nodeList.Item(index);
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block
                .DeclareVar<object>(
                    "value",
                    getter.UnderlyingGetCodegen(Ref("node"), codegenMethodScope, codegenClassScope))
                .MethodReturn(StaticMethod(GetType(), "getXPathNodeListWCheck", Ref("value"), Constant(index)));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var member = codegenClassScope.AddFieldUnshared(
                true,
                typeof(FragmentFactory),
                fragmentFactory.Make(codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(XmlNode), "node")
                .Block
                .DeclareVar<XmlNode>(
                    "result",
                    Cast(typeof(XmlNode), UnderlyingGetCodegen(Ref("node"), codegenMethodScope, codegenClassScope)))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(member, "getEvent", Ref("result")));
        }
    }
} // end of namespace