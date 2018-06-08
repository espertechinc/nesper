///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    ///     Getter for XPath explicit properties returning an element in an array.
    /// </summary>
    public class XPathPropertyArrayItemGetter : EventPropertyGetterSPI
    {
        private readonly FragmentFactory _fragmentFactory;
        private readonly EventPropertyGetterSPI _getter;
        private readonly int _index;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">property getter returning the parent node</param>
        /// <param name="index">to get item at</param>
        /// <param name="fragmentFactory">for creating fragments, or null if not creating fragments</param>
        public XPathPropertyArrayItemGetter(EventPropertyGetterSPI getter, int index, FragmentFactory fragmentFactory)
        {
            _getter = getter;
            _index = index;
            _fragmentFactory = fragmentFactory;
        }

        public object Get(EventBean eventBean)
        {
            return GetXPathNodeListWCheck(_getter.Get(eventBean), _index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static object GetXPathNodeListWCheck(object @object, int index)
        {
            if (@object is XmlNodeList)
            {
                var nodeList = (XmlNodeList) @object;
                if (nodeList.Count <= index) return null;

                return nodeList.Item(index);
            }

            if (@object is IEnumerable<XElement>)
            {
                var nodeList = (IEnumerable<XElement>) @object;
                return nodeList.Skip(index).FirstOrDefault();
            }

            return null;
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context
                .AddMethod(typeof(object), typeof(XmlNode), "node", GetType())
                .DeclareVar(typeof(object), "value", _getter.CodegenUnderlyingGet(Ref("node"), context))
                .MethodReturn(StaticMethod(GetType(), "GetXPathNodeListWCheck",
                    Ref("value"),
                    Constant(_index)));
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentFactory == null) return null;
            var result = (XmlNode) Get(eventBean);
            if (result == null) return null;
            return _fragmentFactory.GetEvent(result);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(FragmentFactory), _fragmentFactory);
            return context.AddMethod(typeof(object), typeof(XmlNode), "node", GetType())
                .DeclareVar(typeof(XmlNode), "result",
                    Cast(typeof(XmlNode), CodegenUnderlyingGet(Ref("node"), context)))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(
                    Ref(member.MemberName), "GetEvent", Ref("result")));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(XmlNode), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            if (_fragmentFactory == null)
            {
                return ConstantNull();
            }

            return CodegenUnderlyingFragment(CastUnderlying(typeof(XmlNode), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            if (_fragmentFactory == null)
            {
                return ConstantNull();
            }

            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
}