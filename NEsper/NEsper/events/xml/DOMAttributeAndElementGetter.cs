///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    ///     Getter for both attribute and element values, attributes are checked first.
    /// </summary>
    public class DOMAttributeAndElementGetter : EventPropertyGetterSPI, DOMPropertyGetter
    {
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        public DOMAttributeAndElementGetter(string propertyName)
        {
            _propertyName = propertyName;
        }

        public object GetValueAsFragment(XmlNode node)
        {
            return null;
        }

        public object GetValueAsFragment(XObject node)
        {
            return null;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            return null;
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetNodePropertyValue(node, _propertyName);
        }

        public XObject GetValueAsNode(XObject node)
        {
            return GetNodePropertyValue(node, _propertyName);
        }

        public object Get(EventBean obj)
        {
            // The underlying is expected to be a map
            if (!(obj.Underlying is XmlNode))
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                  "the underlying data object is not of type Node");
            var node = (XmlNode) obj.Underlying;
            return GetValueAsNode(node);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            // The underlying is expected to be a map
            if (!(eventBean.Underlying is XmlNode))
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                  "the underlying data object is not of type Node");

            var node = (XmlNode) eventBean.Underlying;
            return GetNodePropertyExists(node, _propertyName);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null; // Never a fragment
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(XmlNode), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(XmlNode), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "getNodePropertyValue",
                underlyingExpression, _propertyName);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "getNodePropertyExists",
                underlyingExpression, _propertyName);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }
        
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static XmlNode GetNodePropertyValue(XmlNode node, string propertyName)
        {
            var namedNodeMap = node.Attributes;
            if (namedNodeMap != null)
                for (var i = 0; i < namedNodeMap.Count; i++)
                {
                    var attrNode = namedNodeMap.Item(i);
                    if (attrNode.LocalName != string.Empty)
                    {
                        if (propertyName == attrNode.LocalName) return attrNode;
                        continue;
                    }

                    if (propertyName == attrNode.Name)
                        return attrNode;
                }

            var list = node.ChildNodes;
            for (var i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if (childNode?.NodeType != XmlNodeType.Element) continue;
                if (childNode.LocalName != string.Empty)
                {
                    if (childNode.LocalName == propertyName)
                        return childNode;
                    continue;
                }

                if (childNode.Name == propertyName)
                    return childNode;
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static bool GetNodePropertyExists(XmlNode node, string propertyName)
        {
            var namedNodeMap = node.Attributes;
            if (namedNodeMap != null)
                for (var i = 0; i < namedNodeMap.Count; i++)
                {
                    var attrNode = namedNodeMap.Item(i);
                    if (attrNode.LocalName != string.Empty)
                    {
                        if (propertyName == attrNode.LocalName) return true;
                        continue;
                    }

                    if (propertyName == attrNode.Name) return true;
                }

            var list = node.ChildNodes;
            for (var i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if (childNode == null) continue;
                if (childNode.NodeType != XmlNodeType.Element) continue;
                if (childNode.LocalName != string.Empty)
                {
                    if (propertyName == childNode.LocalName) return true;
                    continue;
                }

                if (childNode.Name == propertyName) return true;
            }

            return false;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static XObject GetNodePropertyValue(XObject node, string propertyName)
        {
            var element = (XElement) node;
            var namedNodeMap = element.Attributes();
            foreach (var attrNode in namedNodeMap)
            {
                if (!string.IsNullOrEmpty(attrNode.Name.LocalName))
                {
                    if (propertyName == attrNode.Name.LocalName)
                    {
                        return attrNode;
                    }
                }
            }

            var list = element.Nodes()
                .Where(c => c != null)
                .Where(c => c.NodeType == XmlNodeType.Element)
                .Cast<XElement>();

            foreach (var childNode in list)
            {
                if (!string.IsNullOrEmpty(childNode.Name.LocalName))
                {
                    if (childNode.Name.LocalName == propertyName)
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static bool GetNodePropertyExists(XObject node, string propertyName)
        {
            var element = (XElement) node;
            var namedNodeMap = element.Attributes();
            foreach (var attrNode in namedNodeMap)
            {
                if (!string.IsNullOrEmpty(attrNode.Name.LocalName))
                {
                    if (propertyName == attrNode.Name.LocalName)
                    {
                        return true;
                    }
                }
            }

            var list = element.Nodes()
                .Where(c => c != null)
                .Where(c => c.NodeType == XmlNodeType.Element)
                .Cast<XElement>();

            foreach (var childNode in list)
            {
                if (!string.IsNullOrEmpty(childNode.Name.LocalName))
                {
                    if (childNode.Name.LocalName == propertyName)
                    {
                        return true;
                    }
                }
            }

            return false;
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
            return ConstantNull();
        }
    }
} // end of namespace