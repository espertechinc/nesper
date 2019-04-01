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
	/// DOM getter for Map-property.
	/// </summary>
	public class DOMMapGetter : EventPropertyGetterSPI, DOMPropertyGetter {
	    private readonly string propertyMap;
	    private readonly string mapKey;
	    private readonly FragmentFactorySPI fragmentFactory;

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="node">node</param>
	    /// <param name="propertyMap">property</param>
	    /// <param name="mapKey">key</param>
	    /// <returns>value</returns>
	    public static XmlNode GetNodeValue(XmlNode node, string propertyMap, string mapKey) {
	        XmlNodeList list = node.ChildNodes;
	        for (int i = 0; i < list.Length; i++) {
	            XmlNode childNode = list.Item(i);
	            if (childNode == null) {
	                continue;
	            }
	            if (childNode.NodeType != XmlNodeType.Element) {
	                continue;
	            }
	            if (!(childNode.NodeName.Equals(propertyMap))) {
	                continue;
	            }

	            XmlNode attribute = childNode.Attributes.GetNamedItem("id");
	            if (attribute == null) {
	                continue;
	            }
	            if (!(attribute.TextContent.Equals(mapKey))) {
	                continue;
	            }

	            return childNode;
	        }
	        return null;
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="node">node</param>
	    /// <param name="propertyMap">property</param>
	    /// <param name="mapKey">key</param>
	    /// <returns>exists flag</returns>
	    public static bool GetNodeValueExists(XmlNode node, string propertyMap, string mapKey) {
	        XmlNodeList list = node.ChildNodes;
	        for (int i = 0; i < list.Length; i++) {
	            XmlNode childNode = list.Item(i);
	            if (childNode == null) {
	                continue;
	            }
	            if (childNode.NodeType != XmlNodeType.Element) {
	                continue;
	            }
	            if (!(childNode.NodeName.Equals(propertyMap))) {
	                continue;
	            }

	            XmlNode attribute = childNode.Attributes.GetNamedItem("id");
	            if (attribute == null) {
	                continue;
	            }
	            if (!(attribute.TextContent.Equals(mapKey))) {
	                continue;
	            }

	            return true;
	        }
	        return false;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="propertyName">property name</param>
	    /// <param name="mapKey">key in map</param>
	    /// <param name="fragmentFactory">for creating fragments</param>
	    public DOMMapGetter(string propertyName, string mapKey, FragmentFactorySPI fragmentFactory) {
	        this.propertyMap = propertyName;
	        this.mapKey = mapKey;
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
	        return codegenMethodScope.MakeChild(typeof(object), this.GetType(), codegenClassScope).AddParam(typeof(XmlNode), "node").Block
	                .DeclareVar(typeof(XmlNode), "result", GetValueAsNodeCodegen(@Ref("node"), codegenMethodScope, codegenClassScope))
	                .IfRefNullReturnNull("result")
	                .MethodReturn(ExprDotMethod(member, "getEvent", @Ref("result")));
	    }

	    public XmlNode GetValueAsNode(XmlNode node) {
	        return GetNodeValue(node, propertyMap, mapKey);
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
	        return GetNodeValueExists(node, propertyMap, mapKey);
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
	        return StaticMethod(this.GetType(), "getNodeValue", underlyingExpression, Constant(propertyMap), Constant(mapKey));
	    }

	    public CodegenExpression UnderlyingExistsCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return StaticMethod(this.GetType(), "getNodeValueExists", underlyingExpression, Constant(propertyMap), Constant(mapKey));
	    }

	    public CodegenExpression UnderlyingFragmentCodegen(CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression GetValueAsNodeCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return UnderlyingGetCodegen(value, codegenMethodScope, codegenClassScope);
	    }

	    public CodegenExpression GetValueAsNodeArrayCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public CodegenExpression GetValueAsFragmentCodegen(CodegenExpression value, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return LocalMethod(GetValueAsFragmentCodegen(codegenMethodScope, codegenClassScope), value);
	    }
	}
} // end of namespace