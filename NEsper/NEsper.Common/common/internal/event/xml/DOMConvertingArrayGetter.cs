///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
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
	/// Getter for converting a Node child nodes into an array.
	/// </summary>
	public class DOMConvertingArrayGetter : EventPropertyGetterSPI {
	    private readonly DOMPropertyGetter getter;
	    private readonly Type componentType;
	    private readonly SimpleTypeParserSPI parser;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="domPropertyGetter">getter</param>
	    /// <param name="returnType">component type</param>
	    public DOMConvertingArrayGetter(DOMPropertyGetter domPropertyGetter, Type returnType) {
	        this.getter = domPropertyGetter;
	        this.componentType = returnType;
	        this.parser = SimpleTypeParserFactory.GetParser(returnType);
	    }

	    public object Get(EventBean obj) {
	        // The underlying is expected to be a map
	        if (!(obj.Underlying is XmlNode)) {
	            throw new PropertyAccessException("Mismatched property getter to event bean type, " +
	                    "the underlying data object is not of type Node");
	        }
	        XmlNode node = (XmlNode) obj.Underlying;
	        XmlNode[] result = getter.GetValueAsNodeArray(node);
	        if (result == null) {
	            return null;
	        }
	        return GetDOMArrayFromNodes(result, componentType, parser);
	    }

	    private CodegenMethod GetCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField mComponentType = codegenClassScope.AddFieldUnshared(true, typeof(Type), Constant(componentType));
	        CodegenExpressionField mParser = codegenClassScope.AddOrGetFieldSharable(new SimpleTypeParserCodegenFieldSharable(parser, codegenClassScope));
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(XmlNode), "node").Block
	                .DeclareVar(typeof(XmlNode[]), "result", getter.GetValueAsNodeArrayCodegen(@Ref("node"), codegenMethodScope, codegenClassScope))
	                .IfRefNullReturnNull("result")
	                .MethodReturn(StaticMethod(this.GetType(), "getDOMArrayFromNodes", @Ref("result"), mComponentType, mParser));
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="result">nodes</param>
	    /// <param name="componentType">type</param>
	    /// <param name="parser">parser</param>
	    /// <returns>result</returns>
	    public static object GetDOMArrayFromNodes(XmlNode[] result, Type componentType, SimpleTypeParser parser) {
	        var array = Array.CreateInstance(componentType, result.Length);
	        for (int i = 0; i < result.Length; i++) {
	            string text = result[i].TextContent;
	            if ((text == null) || (text.Length == 0)) {
	                continue;
	            }

	            object parseResult = parser.Parse(text);
	            array.SetValue(parseResult, i);
	        }

	        return array;
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
	        return ConstantTrue();
	    }

	    public CodegenExpression EventBeanFragmentCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantTrue();
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }
	}
} // end of namespace