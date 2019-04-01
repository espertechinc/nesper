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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
	/// <summary>
	/// Getter for parsing node content to a desired type.
	/// </summary>
	public class DOMConvertingGetter : EventPropertyGetterSPI {
	    private readonly DOMPropertyGetter getter;
	    private readonly SimpleTypeParserSPI parser;

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="node">node</param>
	    /// <param name="parser">parser</param>
	    /// <returns>value</returns>
	    /// <throws>PropertyAccessException exception</throws>
	    public static object GetParseTextValue(XmlNode node, SimpleTypeParser parser) {
	        if (node == null) {
	            return null;
	        }
	        string text = node.TextContent;
	        if (text == null) {
	            return null;
	        }
	        return parser.Parse(text);
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="domPropertyGetter">getter</param>
	    /// <param name="returnType">desired result type</param>
	    public DOMConvertingGetter(DOMPropertyGetter domPropertyGetter, Type returnType) {
	        this.getter = domPropertyGetter;
	        this.parser = SimpleTypeParserFactory.GetParser(returnType);
	    }

	    public object Get(EventBean obj) {
	        // The underlying is expected to be a map
	        if (!(obj.Underlying is XmlNode)) {
	            throw new PropertyAccessException("Mismatched property getter to event bean type, " +
	                    "the underlying data object is not of type Node");
	        }
	        XmlNode node = (XmlNode) obj.Underlying;
	        XmlNode result = getter.GetValueAsNode(node);
	        return GetParseTextValue(result, parser);
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        return true;
	    }

	    public object GetFragment(EventBean eventBean) {
	        return null;
	    }

	    public CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingExistsCodegen(CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression EventBeanFragmentCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField parserMember = codegenClassScope.AddFieldUnshared(true, typeof(SimpleTypeParser), SimpleTypeParserFactory.CodegenSimpleParser(parser, codegenClassScope.PackageScope.InitMethod, this.GetType(), codegenClassScope));
	        CodegenExpression inner = getter.UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
	        return StaticMethod(this.GetType(), "getParseTextValue", inner, parserMember);
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }
	}
} // end of namespace