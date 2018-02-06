///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for converting a Node child nodes into an array.
    /// </summary>
    public class DOMConvertingArrayGetter : EventPropertyGetterSPI
    {
        private readonly Type _componentType;
        private readonly DOMPropertyGetter _getter;
        private readonly SimpleTypeParser _parser;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="domPropertyGetter">getter</param>
        /// <param name="returnType">component type</param>
        public DOMConvertingArrayGetter(DOMPropertyGetter domPropertyGetter, Type returnType)
        {
            _getter = domPropertyGetter;
            _componentType = returnType;
            _parser = SimpleTypeParserFactory.GetParser(returnType);
        }

        public Object Get(EventBean eventBean)
        {
            var asXml = eventBean.Underlying as XmlNode;
            if (asXml == null)
            {
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                  "the underlying data object is not of type Node");
            }

            var result = _getter.GetValueAsNodeArray(asXml);
            if (result == null)
            {
                return null;
            }

            return GetDOMArrayFromNodes(result, _componentType, _parser);
        }

        private String GetCodegen(ICodegenContext context)
        {
            var mComponentType = context.MakeAddMember(typeof(Type), _componentType);
            var mParser = context.MakeAddMember(typeof(SimpleTypeParser), _parser);
            return context.AddMethod(typeof(object), typeof(object), "node", GetType())
                .DeclareVar(typeof(object[]), "result", _getter.GetValueAsNodeArrayCodegen(Ref("node"), context))
                .IfRefNullReturnNull("result")
                .MethodReturn(
                    StaticMethod(
                        GetType(), "GetDOMArrayFromNodes", 
                        Ref("result"),
                        Ref(mComponentType.MemberName),
                        Ref(mParser.MemberName)));
        }

        private static String GetInnerText(object value)
        {
            if (value is XmlNode node)
            {
                return node.InnerText;
            }
            else if (value is XContainer container)
            {
                return string.Concat(container.Nodes());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static object GetDOMArrayFromNodes(object[] result, Type componentType, SimpleTypeParser parser)
        {
            var array = Array.CreateInstance(componentType, result.Length);
            for (int i = 0; i < result.Length; i++) {
                var text = GetInnerText(result[i]);
                if (string.IsNullOrEmpty(text)) {
                    continue;
                }

                var parseResult = parser.Invoke(text);
                array.SetValue(parseResult, i);
            }

            return array;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(object), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
}
