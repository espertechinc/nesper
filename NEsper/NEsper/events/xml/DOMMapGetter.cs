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
    /// DOM getter for Map-property.
    /// </summary>
    public class DOMMapGetter
        : EventPropertyGetterSPI
        , DOMPropertyGetter
    {
        private readonly FragmentFactory _fragmentFactory;
        private readonly String _mapKey;
        private readonly String _propertyMap;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="mapKey">key in map</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMMapGetter(String propertyName, String mapKey, FragmentFactory fragmentFactory)
        {
            _propertyMap = propertyName;
            _mapKey = mapKey;
            _fragmentFactory = fragmentFactory;
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            return GetNodeValueXml(node, _propertyMap, _mapKey);
        }

        public XObject GetValueAsNode(XObject node)
        {
            return GetNodeValueLinq(node, _propertyMap, _mapKey);
        }

        public object GetValueAsNode(object underlying)
        {
            return GetNodeValue(underlying, _propertyMap, _mapKey);
        }

        public object Get(EventBean eventBean)
        {
            return GetValueAsNode(eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return GetNodeValueExists(eventBean.Underlying, _propertyMap, _mapKey);
        }

        public Object GetFragment(EventBean eventBean)
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

        #region GetValueAsFragment

        public object GetValueAsFragment(XmlNode node)
        {
            if (_fragmentFactory == null)
                return null;

            return GetValueAsNode(node) is XmlNode result ? _fragmentFactory.GetEvent(result) : null;
        }

        public object GetValueAsFragment(XObject node)
        {
            if (_fragmentFactory == null)
                return null;

            return GetValueAsNode(node) is XObject result ? _fragmentFactory.GetEvent(result) : null;
        }

        #endregion

        #region GetNodeValue

        private static XmlNode GetNodeValueXml(XmlNode node, String propertyMap, String mapKey)
        {
            XmlNodeList list = node.ChildNodes;
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode childNode = list.Item(i);
                if ((childNode != null) &&
                    (childNode.NodeType == XmlNodeType.Element) &&
                    (childNode.Name == propertyMap) &&
                    (childNode.Attributes != null))
                {
                    XmlNode attribute = childNode.Attributes.GetNamedItem("id");
                    if ((attribute != null) &&
                        (attribute.InnerText == mapKey))
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        private static XObject GetNodeValueLinq(XObject node, String propertyMap, String mapKey)
        {
            if (node is XContainer asContainer) {
                var elements = asContainer.Elements()
                    .Where(e => e.Name.LocalName == propertyMap);
                foreach (var element in elements) {
                    var result = element.Attributes()
                        .FirstOrDefault(attribute => attribute.Name.LocalName == "id" && attribute.Value == mapKey);
                    if (result != null) {
                        return element;
                    }
                }
            }

            return null;
        }

        public static object GetNodeValue(object underlyingValue, String propertyMap, String mapKey)
        {
            if (underlyingValue is XElement element)
            {
                return GetNodeValueLinq(element, propertyMap, mapKey);
            }
            else if (underlyingValue is XmlNode node)
            {
                return GetNodeValueXml(node, propertyMap, mapKey);
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region GetNodeValueExists

        private static bool GetNodeValueExists(XmlNode node, String propertyMap, String mapKey)
        {
            XmlNodeList list = node.ChildNodes;
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode childNode = list.Item(i);
                if ((childNode != null) &&
                    (childNode.NodeType == XmlNodeType.Element) &&
                    (childNode.Name == propertyMap) &&
                    (childNode.Attributes != null))
                {
                    XmlNode attribute = childNode.Attributes.GetNamedItem("id");
                    if ((attribute != null) &&
                        (attribute.InnerText == mapKey))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetNodeValueExists(XElement asXNode, String propertyMap, String mapKey)
        {
            return asXNode.Elements()
                .Where(e => e.Name.LocalName == propertyMap)
                .Select(element => element.Attributes().FirstOrDefault(attr => attr.Name.LocalName == mapKey))
                .Where(attribute => attribute != null)
                .Any(attribute => attribute.Value == mapKey);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="underlyingValue">The underlying value.</param>
        /// <param name="propertyMap">The property map.</param>
        /// <param name="mapKey">The map key.</param>
        /// <returns></returns>
        public static bool GetNodeValueExists(object underlyingValue, String propertyMap, String mapKey)
        {
            if (underlyingValue is XElement element)
            {
                return GetNodeValueExists(element, propertyMap, mapKey);
            }
            else if (underlyingValue is XmlNode node)
            {
                return GetNodeValueExists(node, propertyMap, mapKey);
            }
            else
            {
                return false;
            }
        }

        #endregion

        private String GetValueAsFragmentCodegen(ICodegenContext context)
        {
            var mType = context.MakeAddMember(typeof(FragmentFactory), _fragmentFactory);
            return context
                .AddMethod(typeof(object), typeof(object), "node", GetType())
                .DeclareVar(typeof(object), "result", GetValueAsNodeCodegen(Ref("node"), context))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(Ref(mType.MemberName), "GetEvent", Ref("result")));
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
            return StaticMethodTakingExprAndConst(this.GetType(), "GetNodeValue", underlyingExpression, _propertyMap, _mapKey);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(this.GetType(), "GetNodeValueExists", underlyingExpression, _propertyMap, _mapKey);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
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
            return LocalMethod(GetValueAsFragmentCodegen(context), value);
        }
    }
}
