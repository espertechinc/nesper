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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
	/// <summary>
	/// Getter for retrieving a value at a certain index.
	/// </summary>
	public class DOMIndexedGetter : EventPropertyGetterSPI, DOMPropertyGetter {
	    private readonly string propertyName;
	    private readonly int index;
	    private readonly FragmentFactoryDOMGetter fragmentFactory;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="propertyName">property name</param>
	    /// <param name="index">index</param>
	    /// <param name="fragmentFactory">for creating fragments if required</param>
	    public DOMIndexedGetter(string propertyName, int index, FragmentFactoryDOMGetter fragmentFactory) {
	        this.propertyName = propertyName;
	        this.index = index;
	        this.fragmentFactory = fragmentFactory;
	    }

	    public XmlNode[] GetValueAsNodeArray(XmlNode node) {
	        return null;
	    }

	    public object GetValueAsFragment(XmlNode node) {
	        if (fragmentFactory == null) {
	            return null;
	        }
	        XmlNode result = GetValueAsNode(node);
	        if (result == null) {
	            return null;
	        }
	        return fragmentFactory.GetEvent(result);
	    }

	    private CodegenMethod GetValueAsFragmentCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField member = codegenClassScope.AddFieldUnshared(true, typeof(FragmentFactory), fragmentFactory.Make(codegenClassScope.PackageScope.InitMethod, codegenClassScope));
	        CodegenMethod method = codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(XmlNode), "node");
	        method.Block
	                .DeclareVar(typeof(XmlNode), "result", StaticMethod(typeof(DOMIndexedGetter), "getNodeValue", @Ref("node"), Constant(propertyName), Constant(index)))
	                .IfRefNullReturnNull("result")
	                .MethodReturn(ExprDotMethod(member, "getEvent", @Ref("result")));
	        return method;
	    }

	    public XmlNode GetValueAsNode(XmlNode node) {
	        return GetNodeValue(node, propertyName, index);
	    }

	    public object Get(EventBean eventBean) {
	        object result = eventBean.Underlying;
	        if (!(result is XmlNode)) {
	            return null;
	        }
	        XmlNode node = (XmlNode) result;
	        return GetValueAsNode(node);
	    }

	    public bool IsExistsProperty(EventBean eventBean) {
	        object result = eventBean.Underlying;
	        if (!(result is XmlNode)) {
	            return false;
	        }
	        XmlNode node = (XmlNode) result;
	        return GetValueAsNode(node) != null;
	    }

	    public object GetFragment(EventBean eventBean) {
	        object result = eventBean.Underlying;
	        if (!(result is XmlNode)) {
	            return null;
	        }
	        XmlNode node = (XmlNode) result;
	        return GetValueAsFragment(node);
	    }

	    public CodegenExpression EventBeanGetCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression EventBeanExistsCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingExistsCodegen(CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression EventBeanFragmentCodegen(CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingFragmentCodegen(CastUnderlying(typeof(XmlNode), beanExpression), codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression UnderlyingGetCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return StaticMethod(this.GetType(), "getNodeValue", underlyingExpression, Constant(propertyName), Constant(index));
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return StaticMethod(this.GetType(), "getNodeValueExists", underlyingExpression, Constant(propertyName), Constant(index));
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        if (fragmentFactory == null) {
	            return ConstantNull();
	        }
	        return LocalMethod(GetValueAsFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
	    }

	    public CodegenExpression GetValueAsNodeCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(value, codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression GetValueAsNodeArrayCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression GetValueAsFragmentCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingFragmentCodegen(value, codegenMethodScope, codegenClassScope);
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="node">node</param>
	    /// <param name="propertyName">property</param>
	    /// <param name="index">index</param>
	    /// <returns>value</returns>
	    public static XmlNode GetNodeValue(XmlNode node, string propertyName, int index) {
	        XmlNodeList list = node.ChildNodes;
	        int count = 0;
	        for (int i = 0; i < list.Count; i++) {
	            XmlNode childNode = list.Item(i);
	            if (childNode == null) {
	                continue;
	            }
	            if (childNode.NodeType != XmlNodeType.Element) {
	                continue;
	            }

	            string elementName = childNode.LocalName;
	            if (elementName == null) {
	                elementName = childNode.Name;
	            }

	            if (!(propertyName.Equals(elementName))) {
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
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="node">node</param>
	    /// <param name="propertyName">property</param>
	    /// <param name="index">index</param>
	    /// <returns>value</returns>
	    public static bool GetNodeValueExists(XmlNode node, string propertyName, int index) {
	        return GetNodeValue(node, propertyName, index) != null;
	    }
	}
} // end of namespace