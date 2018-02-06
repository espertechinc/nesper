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
    /// <summary>Getter for simple attributes in a DOM node.</summary>
    public class DOMSimpleAttributeGetter : EventPropertyGetterSPI, DOMPropertyGetter
    {
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        public DOMSimpleAttributeGetter(string propertyName)
        {
            this._propertyName = propertyName;
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

        public object Get(EventBean eventBean)
        {
            var xnode = eventBean.Underlying as XNode;
            if (xnode == null)
            {
                var node = eventBean.Underlying as XmlNode;
                if (node == null)
                {
                    throw new PropertyAccessException(
                        "Mismatched property getter to event bean type, " +
                        "the underlying data object is not of type Node");
                }

                return GetValueAsNode(node);
            }

            return GetValueAsNode(xnode);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
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
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethod(GetType(), "GetNodePropertyValue", underlyingExpression,
                Constant(_propertyName));
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

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>node</returns>
        public static XObject GetNodePropertyValue(XObject node, string propertyName)
        {
            var element = (XElement) node;
            foreach (var attrNode in element.Attributes())
            {
                if (!string.IsNullOrEmpty(attrNode.Name.LocalName))
                {
                    if (propertyName == attrNode.Name.LocalName)
                        return attrNode;
                    continue;
                }

                if (propertyName == attrNode.Name)
                    return attrNode;
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="propertyName">property</param>
        /// <returns>node</returns>
        public static XmlNode GetNodePropertyValue(XmlNode node, string propertyName)
        {
            if (node.Attributes != null)
            {
                foreach (var attrNode in node.Attributes.Cast<XmlAttribute>())
                {
                    if (!string.IsNullOrEmpty(attrNode.LocalName))
                    {
                        if (propertyName == attrNode.LocalName)
                            return attrNode;
                        continue;
                    }

                    if (propertyName == attrNode.Name)
                        return attrNode;
                }
            }

            return null;
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