///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    /// Getter for retrieving a value at a certain index.
    /// </summary>
    public class DOMIndexedGetter 
        : EventPropertyGetterSPI
        , DOMPropertyGetter
    {
        private readonly String _propertyName;
        private readonly int _index;
        private readonly FragmentFactory _fragmentFactory;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">index</param>
        /// <param name="fragmentFactory">for creating fragments if required</param>
        public DOMIndexedGetter(String propertyName, int index, FragmentFactory fragmentFactory)
        {
            _propertyName = propertyName;
            _index = index;
            _fragmentFactory = fragmentFactory;
        }

        #region GetNodeValue

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static object GetNodeValue(object node, String propertyName, int index)
        {
            if (node is XmlNode xmlnode)
            {
                return GetNodeValue(xmlnode, propertyName, index);
            }
            else if (node is XContainer container)
            {
                return GetNodeValue(container, propertyName, index);
            }
            else
            {
                return null;
            }
        }

        public static XmlNode GetNodeValue(XmlNode node, String propertyName, int index)
        {
            var list = node.ChildNodes;
            var count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if ((childNode != null) && (childNode.NodeType == XmlNodeType.Element))
                {
                    var elementName = childNode.LocalName;
                    if ((elementName == propertyName) && (count == index))
                    {
                        return childNode;
                    }

                    count++;
                }
            }

            return null;
        }

        public static XObject GetNodeValue(XContainer container, String propertyName, int index)
        {
            if (container == null)
                return null;

            return container
                .Nodes()
                .Where(c => c != null)
                .Where(c => c.NodeType == XmlNodeType.Element)
                .Cast<XElement>()
                .Where(c => c.Name.LocalName == propertyName)
                .Skip(index)
                .FirstOrDefault();
        }

        #endregion

        #region GetNodeValueExists

        public static bool GetNodeValueExists(object node, String propertyName, int index)
        {
            return GetNodeValue(node, propertyName, index) != null;
        }

        #endregion

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            return null;
        }

        public object GetValueAsFragment(XmlNode node)
        {
            if (_fragmentFactory != null)
            {
                var result = GetValueAsNode(node);
                if (result != null)
                {
                    return _fragmentFactory.GetEvent(result);
                }
            }

            return null;
        }

        public object GetValueAsFragment(XObject node)
        {
            if (_fragmentFactory != null)
            {
                var result = GetValueAsNode(node);
                if (result != null)
                {
                    return _fragmentFactory.GetEvent(result);
                }
            }

            return null;
        }

        private String GetValueAsFragmentCodegen(ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(FragmentFactory), _fragmentFactory);
            return context
                .AddMethod(typeof(object), typeof(object), "node", GetType())
                .DeclareVar(typeof(object), "result", StaticMethod(
                    typeof(DOMIndexedGetter), "GetNodeValue", 
                    Ref("node"), 
                    Constant(_propertyName), 
                    Constant(_index)))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(Ref(member.MemberName), "GetEvent", Ref("result")));
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetNodeValue(node, _propertyName, _index);
        }

        public XObject GetValueAsNode(XObject node)
        {
            return GetNodeValue(node as XContainer, _propertyName, _index);
        }

        public object Get(EventBean eventBean)
        {
            if (eventBean.Underlying is XNode xnode)
            {
                return GetValueAsNode(xnode);
            }
            else if (eventBean.Underlying is XmlNode node)
            {
                return GetValueAsNode(node);
            }
            else
            {
                return null;
            }
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (eventBean.Underlying is XNode xnode)
            {
                return GetValueAsNode(xnode) != null;
            }
            else if (eventBean.Underlying is XmlNode node)
            {
                return GetValueAsNode(node) != null;
            }
            else
            {
                return false;
            }
        }
    
        public object GetFragment(EventBean eventBean)
        {
            if (eventBean.Underlying is XNode xnode)
            {
                return GetValueAsFragment(xnode);
            }
            else if (eventBean.Underlying is XmlNode node)
            {
                return GetValueAsFragment(node);
            }
            else
            {
                return null;
            }
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
            return CodegenUnderlyingFragment(CastUnderlying(typeof(object), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetNodeValue", underlyingExpression, _propertyName, _index);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "GetNodeValueExists", underlyingExpression, _propertyName, _index);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            if (_fragmentFactory == null)
            {
                return ConstantNull();
            }
            return LocalMethod(GetValueAsFragmentCodegen(context), underlyingExpression);
        }

        public ICodegenExpression GetValueAsNodeCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return CodegenUnderlyingGet(value, context);
        }

        public ICodegenExpression GetValueAsNodeArrayCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression GetValueAsFragmentCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(value, context);
        }

    }
}
