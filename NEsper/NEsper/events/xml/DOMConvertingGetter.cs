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
    ///     Getter for parsing node content to a desired type.
    /// </summary>
    public class DOMConvertingGetter : EventPropertyGetterSPI
    {
        private readonly DOMPropertyGetter _getter;
        private readonly SimpleTypeParser _parser;
        private ICodegenMember _codegenParser;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="domPropertyGetter">getter</param>
        /// <param name="returnType">desired result type</param>
        public DOMConvertingGetter(DOMPropertyGetter domPropertyGetter, Type returnType)
        {
            _getter = domPropertyGetter;
            _parser = SimpleTypeParserFactory.GetParser(returnType);
        }

        public object Get(EventBean eventBean)
        {
            if (eventBean.Underlying is XNode xnode)
            {
                var result = _getter.GetValueAsNode(xnode);
                if (result != null)
                    return GetParseTextValue(result, _parser);
                return null;
            }
            else if (eventBean.Underlying is XmlNode xmlnode)
            {
                var result = _getter.GetValueAsNode(xmlnode);
                if (result != null)
                    return GetParseTextValue(result, _parser);
                return null;
            }
            else
            {
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                  "the underlying data object is not of type Node");
            }
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(object), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(object), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            GenerateParserMember(context);
            ICodegenExpression inner = _getter.CodegenUnderlyingGet(underlyingExpression, context);
            return StaticMethod(GetType(), "GetParseTextValue", inner,
                Ref(_codegenParser.MemberName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        #region GetParseTextValue

        private static object GetParseTextValue(XmlNode node, SimpleTypeParser parser)
        {
            var text = node?.InnerText;
            if (text == null)
                return null;

            return parser.Invoke(text);
        }

        private static object GetParseTextValue(XObject node, SimpleTypeParser parser)
        {
            if (node is XContainer container)
            {
                return string.Concat(container.Nodes());
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static object GetParseTextValue(object node, SimpleTypeParser parser)
        {
            if (node is XObject xobject)
            {
                return GetParseTextValue(xobject, parser);
            }
            else if (node is XmlNode xmlnode)
            {
                return GetParseTextValue(xmlnode, parser);
            }
            else
            {
                return null;
            }
        }


        #endregion

        private void GenerateParserMember(ICodegenContext context)
        {
            if (_codegenParser == null) _codegenParser = context.MakeMember(typeof(SimpleTypeParser), _parser);
            context.AddMember(_codegenParser);
        }
    }
}