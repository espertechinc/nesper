///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for nested properties in a DOM tree.
    /// </summary>
    public class DOMNestedPropertyGetter : EventPropertyGetterSPI
        , DOMPropertyGetter
    {
        private readonly DOMPropertyGetter[] _domGetterChain;
        private readonly FragmentFactory _fragmentFactory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMNestedPropertyGetter(ICollection<EventPropertyGetter> getterChain, FragmentFactory fragmentFactory)
        {
            this._domGetterChain = new DOMPropertyGetter[getterChain.Count];
            this._fragmentFactory = fragmentFactory;

            int count = 0;
            foreach (EventPropertyGetter getter in getterChain)
            {
                _domGetterChain[count++] = (DOMPropertyGetter) getter;
            }
        }

        public Object GetValueAsFragment(XmlNode node)
        {
            var result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        public object GetValueAsFragment(XObject node)
        {
            var result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        private String GetValueAsFragmentCodegen<T>(ICodegenContext context)
        {
            var scalarType = typeof(T);
            var mType = context.MakeAddMember(typeof(FragmentFactory), _fragmentFactory);
            return context.AddMethod(typeof(object), scalarType, "node", GetType())
                .DeclareVar(scalarType, "result", GetValueAsNodeCodegen(Ref("node"), context))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(
                    Ref(mType.MemberName), "GetEvent",
                    Ref("result")));
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                node = _domGetterChain[i].GetValueAsNode(node);

                if (node == null)
                {
                    return null;
                }
            }

            return _domGetterChain[_domGetterChain.Length - 1].GetValueAsNodeArray(node);
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                node = _domGetterChain[i].GetValueAsNode(node);

                if (node == null)
                {
                    return null;
                }
            }

            return _domGetterChain[_domGetterChain.Length - 1].GetValueAsNodeArray(node);
        }

        private String GetValueAsNodeArrayCodegen<T>(ICodegenContext codegenContext)
        {
            var arrayType = typeof(T[]);
            var scalarType = typeof(T);
            var block = codegenContext.AddMethod(arrayType, scalarType, "node", GetType());
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                block.AssignRef("node", _domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), codegenContext));
                block.IfRefNullReturnNull("node");
            }

            return block.MethodReturn(
                _domGetterChain[_domGetterChain.Length - 1]
                    .GetValueAsNodeArrayCodegen(Ref("node"), codegenContext));
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            XmlNode value = node;

            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);
                if (value == null)
                {
                    return null;
                }
            }

            return value;
        }

        public XObject GetValueAsNode(XObject node)
        {
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                node = _domGetterChain[i].GetValueAsNode(node);
                if (node == null)
                {
                    return null;
                }
            }

            return node;
        }

        private String GetValueAsNodeCodegen<T>(ICodegenContext codegenContext)
        {
            var nodeType = typeof(T);
            var block = codegenContext.AddMethod(nodeType, nodeType, "node", GetType());
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                block.AssignRef("node", _domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), codegenContext));
                block.IfRefNullReturnNull("node");
            }

            return block.MethodReturn(Ref("node"));
        }



        public Object Get(EventBean eventBean)
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

        private bool IsExistsProperty(XObject valueX)
        {
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                valueX = _domGetterChain[i].GetValueAsNode(valueX);
                if (valueX == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsExistsProperty(XmlNode node)
        {
            if (node == null)
            {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type Node");
            }

            var value = node;
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);
                if (value == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsExistsProperty(EventBean obj)
        {
            if (obj.Underlying is XNode xnode)
            {
                return IsExistsProperty(xnode);
            }
            else if (obj.Underlying is XmlNode xmlnode)
            {
                return IsExistsProperty(xmlnode);
            }
            else
            {
                throw new PropertyAccessException(
                    "Mismatched property getter to event bean type, " +
                    "the underlying data object is not of type Node");
            }
        }

        public Object GetFragment(EventBean obj)
        {
            var xnode = obj.Underlying as XNode;
            if (xnode == null)
            {
                var node = obj.Underlying as XmlNode;
                if (node == null)
                {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                var value = node;
                for (int i = 0; i < _domGetterChain.Length - 1; i++)
                {
                    value = _domGetterChain[i].GetValueAsNode(value);

                    if (value == null)
                    {
                        return null;
                    }
                }

                return _domGetterChain[_domGetterChain.Length - 1].GetValueAsFragment(value);
            }
            else
            {
                XObject value = xnode;
                for (int i = 0; i < _domGetterChain.Length - 1; i++)
                {
                    value = _domGetterChain[i].GetValueAsNode(value);
                    if (value == null)
                    {
                        return null;
                    }
                }

                return _domGetterChain[_domGetterChain.Length - 1].GetValueAsFragment(value);
            }
        }

        private String IsExistsPropertyCodegen<T>(ICodegenContext context)
        {
            var scalarType = typeof(T);
            var block = context.AddMethod(typeof(bool), scalarType, "node", GetType());
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                block.AssignRef("node", _domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), context));
                block.IfRefNullReturnFalse("node");
            }

            return block.MethodReturn(ConstantTrue());
        }

        private String GetFragmentCodegen<T>(ICodegenContext context)
        {
            var scalarType = typeof(T);
            var block = context.AddMethod(typeof(object), scalarType, "node", GetType());
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                block.AssignRef("node", _domGetterChain[i].GetValueAsNodeCodegen(Ref("node"), context));
                block.IfRefNullReturnNull("node");
            }

            return block.MethodReturn(_domGetterChain[_domGetterChain.Length - 1].CodegenUnderlyingFragment(
                Ref("node"), context));
        }

        public ICodegenExpression CodegenEventBeanGet<T>(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(T), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenEventBeanGet<object>(beanExpression, context);
        }

        public ICodegenExpression CodegenEventBeanExists<T>(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(T), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenEventBeanExists<object>(beanExpression, context);
        }

        public ICodegenExpression CodegenEventBeanFragment<T>(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(T), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenEventBeanFragment<object>(beanExpression, context);
        }

        public ICodegenExpression CodegenUnderlyingGet<T>(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetValueAsNodeCodegen<T>(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet<object>(underlyingExpression, context);
        }

        public ICodegenExpression CodegenUnderlyingExists<T>(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(IsExistsPropertyCodegen<T>(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists<object>(underlyingExpression, context);
        }

        public ICodegenExpression CodegenUnderlyingFragment<T>(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen<T>(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment<object>(underlyingExpression, context);
        }

        public ICodegenExpression GetValueAsNodeCodegen<T>(ICodegenExpression value, ICodegenContext context)
        {
            return LocalMethod(GetValueAsNodeCodegen<T>(context), value);
        }

        public ICodegenExpression GetValueAsNodeCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return GetValueAsNodeCodegen<object>(value, context);
        }

        public ICodegenExpression GetValueAsNodeArrayCodegen<T>(ICodegenExpression value, ICodegenContext context)
        {
            return LocalMethod(GetValueAsNodeArrayCodegen<T>(context), value);
        }

        public ICodegenExpression GetValueAsNodeArrayCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return GetValueAsNodeArrayCodegen<object>(value, context);
        }

        public ICodegenExpression GetValueAsFragmentCodegen<T>(ICodegenExpression value, ICodegenContext context)
        {
            return LocalMethod(GetValueAsFragmentCodegen<T>(context), value);
        }

        public ICodegenExpression GetValueAsFragmentCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return GetValueAsFragmentCodegen<object>(value, context);
        }
    }
}
