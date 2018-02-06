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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for a DOM complex element.
    /// </summary>
    public class DOMComplexElementGetter
        : EventPropertyGetterSPI
        , DOMPropertyGetter
    {
        private readonly FragmentFactory _fragmentFactory;
        private readonly bool _isArray;
        private readonly String _propertyName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        /// <param name="isArray">if this is an array property</param>
        public DOMComplexElementGetter(String propertyName, FragmentFactory fragmentFactory, bool isArray)
        {
            _propertyName = propertyName;
            _fragmentFactory = fragmentFactory;
            _isArray = isArray;
        }

        #region GetValueAsNode

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static object GetValueAsNode(object node, String propertyName)
        {
            if (node is XContainer container)
            {
                return GetValueAsNode(container, propertyName);
            }
            else if (node is XmlNode xnode)
            {
                return GetValueAsNode(xnode, propertyName);
            }
            else
            {
                return null;
            }
        }
        
        public static XmlNode GetValueAsNode(XmlNode node, String propertyName)
        {
            var list = node.ChildNodes;
            for (int ii = 0; ii < list.Count; ii++)
            {
                var childNode = list.Item(ii);
                if ((childNode != null) && 
                    (childNode.NodeType == XmlNodeType.Element) &&
                    (childNode.LocalName == propertyName))
                {
                    return childNode;
                }
            }
            return null;
        }

        public static XObject GetValueAsNode(XContainer container, String propertyName)
        {
            return container.Elements()
                .FirstOrDefault(e => e.Name.LocalName == propertyName || e.Name == propertyName);
        }

        #endregion

        #region GetValueAsNodeArray

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Array GetValueAsNodeArray(object node, string propertyName)
        {
            if (node is XContainer container)
            {
                return GetValueAsNodeArray(container, propertyName);
            }
            else if (node is XmlNode xnode)
            {
                return GetValueAsNodeArray(xnode, propertyName).UnwrapIntoArray<object>();
            }
            else
            {
                return null;
            }

        }

        private static XmlNode[] GetValueAsNodeArray(XmlNode node, String propertyName)
        {
            var list = node.ChildNodes;

            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if (childNode != null && childNode.NodeType == XmlNodeType.Element)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return new XmlNode[0];
            }

            var nodes = new XmlNode[count];
            int realized = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if ((childNode.NodeType == XmlNodeType.Element) &&
                    (childNode.LocalName == propertyName))
                {
                    nodes[realized++] = childNode;
                }
            }

            if (realized == count)
            {
                return nodes;
            }
            if (realized == 0)
            {
                return new XmlNode[0];
            }

            var shrunk = new XmlNode[realized];
            Array.Copy(nodes, 0, shrunk, 0, realized);
            return shrunk;
        }

        private static XObject[] GetValueAsNodeArray(XContainer container, string propertyName)
        {
            if (container != null)
            {
                return container.Elements()
                    .Where(childNode => childNode.Name.LocalName == propertyName)
                    .Cast<XObject>()
                    .ToArray();
            }

            return null;
        }

        #endregion

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="fragmentFactory">The fragment factory.</param>
        /// <returns></returns>
        public static object GetValueAsNodeFragment(object node, String propertyName, FragmentFactory fragmentFactory)
        {
            var result = GetValueAsNode(node, propertyName);
            if (result is XContainer container)
                return fragmentFactory.GetEvent(container);
            else if (result is XmlNode xnode)
                return fragmentFactory.GetEvent(xnode);
            else
                return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="fragmentFactory">The fragment factory.</param>
        /// <returns></returns>
        public static object GetValueAsNodeFragmentArray(object node, String propertyName, FragmentFactory fragmentFactory)
        {
            var result = GetValueAsNodeArray(node, propertyName);
            if ((result == null) || (result.Length == 0))
            {
                return new EventBean[0];
            }

            var events = new EventBean[result.Length];
            int count = 0;

            for (int ii = 0; ii < result.Length; ii++)
            {
                var item = result.GetValue(ii);
                if (item is XContainer container)
                    events[count++] = fragmentFactory.GetEvent(container);
                else if (item is XmlNode xnode)
                    events[count++] = fragmentFactory.GetEvent(xnode);
                else
                    events[count++] = null;
            }
            return events;
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            var asXNode = eventBean.Underlying as XNode;
            if (asXNode == null) {
                var asXml = eventBean.Underlying as XmlNode;
                if (asXml == null) {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                return _isArray ? (object) GetValueAsNodeArray(asXml) : GetValueAsNode(asXml);
            }

            return _isArray ? (object)GetValueAsNodeArray(asXNode) : GetValueAsNode(asXNode);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean obj)
        {
            var asXNode = obj.Underlying as XNode;
            if (asXNode == null) {
                var asXml = obj.Underlying as XmlNode;
                if (asXml == null) {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                return GetValueAsFragment(asXml);
            }

            return GetValueAsFragment(asXNode);
        }

        #endregion

        public Object GetValueAsFragment(XmlNode node)
        {
            if (_isArray)
                return GetValueAsNodeFragmentArray(node, _propertyName, _fragmentFactory);
            else
                return GetValueAsNodeFragment(node, _propertyName, _fragmentFactory);
        }

        public object GetValueAsFragment(XObject node)
        {
            if (_isArray)
                return GetValueAsNodeFragmentArray(node, _propertyName, _fragmentFactory);
            else
                return GetValueAsNodeFragment(node, _propertyName, _fragmentFactory);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetValueAsNode(node, _propertyName);
        }

        public XObject GetValueAsNode(XObject node)
        {
            return GetValueAsNode(node as XContainer, _propertyName);
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return GetValueAsNodeArray(node, _propertyName);
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            return GetValueAsNodeArray(node as XContainer, _propertyName);
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
            return CodegenUnderlyingFragment(CastUnderlying(typeof(object), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            if (!_isArray)
            {
                return StaticMethod(GetType(), "GetValueAsNode", underlyingExpression, Constant(_propertyName));
            }
            return StaticMethod(GetType(), "GetValueAsNodeArray", underlyingExpression, Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(FragmentFactory), _fragmentFactory);
            if (!_isArray)
            {
                return StaticMethod(GetType(), "GetValueAsNodeFragment", underlyingExpression,
                    Constant(_propertyName), Ref(member.MemberName));
            }
            else
            {
                return StaticMethod(GetType(), "GetValueAsNodeFragmentArray", underlyingExpression,
                    Constant(_propertyName), Ref(member.MemberName));
            }
        }

        public ICodegenExpression GetValueAsNodeCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return StaticMethod(GetType(), "GetValueAsNode", value, Constant(_propertyName));
        }

        public ICodegenExpression GetValueAsNodeArrayCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return StaticMethod(GetType(), "GetValueAsNodeArray", value, Constant(_propertyName));
        }

        public ICodegenExpression GetValueAsFragmentCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(value, context);
        }
    }
}
