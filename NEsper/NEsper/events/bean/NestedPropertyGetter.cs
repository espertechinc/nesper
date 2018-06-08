///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>Getter for one or more levels deep nested properties.</summary>
    public class NestedPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter[] _getterChain;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdapterService">is the cache and factory for event bean types and event wrappers</param>
        /// <param name="finalPropertyType">type of the entry returned</param>
        /// <param name="finalGenericType">generic type parameter of the entry returned, if any</param>
        public NestedPropertyGetter(IList<EventPropertyGetter> getterChain, EventAdapterService eventAdapterService, Type finalPropertyType, Type finalGenericType)
            : base(eventAdapterService, finalPropertyType, finalGenericType)
        {
            this._getterChain = new BeanEventPropertyGetter[getterChain.Count];

            for (int i = 0; i < getterChain.Count; i++)
            {
                this._getterChain[i] = (BeanEventPropertyGetter)getterChain[i];
            }
        }

        public Object GetBeanProp(Object value)
        {
            if (value == null)
            {
                return value;
            }

            for (int i = 0; i < _getterChain.Length; i++)
            {
                value = _getterChain[i].GetBeanProp(value);

                if (value == null)
                {
                    return null;
                }
            }
            return value;
        }

        public bool IsBeanExistsProperty(Object value)
        {
            if (value == null)
            {
                return false;
            }

            int lastElementIndex = _getterChain.Length - 1;

            // walk the getter chain up to the previous-to-last element, returning its object value.
            // any null values in between mean the property does not exists
            for (int i = 0; i < _getterChain.Length - 1; i++)
            {
                value = _getterChain[i].GetBeanProp(value);

                if (value == null)
                {
                    return false;
                }
            }

            return _getterChain[lastElementIndex].IsBeanExistsProperty(value);
        }

        public override Object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return IsBeanExistsProperty(eventBean.Underlying);
        }

        public override Type BeanPropType => _getterChain[_getterChain.Length - 1].BeanPropType;

        public override Type TargetType => _getterChain[0].TargetType;

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetBeanPropCodegen(context, false), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetBeanPropCodegen(context, true), underlyingExpression);
        }

        private string GetBeanPropCodegen(ICodegenContext context, bool exists)
        {
            var block = context.AddMethod(exists
                ? typeof(bool)
                : TypeHelper.GetBoxedType(_getterChain[_getterChain.Length - 1].BeanPropType), _getterChain[0].TargetType, "value", this.GetType());
            if (!exists)
            {
                block.IfRefNullReturnNull("value");
            }
            else
            {
                block.IfRefNullReturnFalse("value");
            }
            string lastName = "value";
            for (int i = 0; i < _getterChain.Length - 1; i++)
            {
                string varName = "l" + i;
                block.DeclareVar(_getterChain[i].BeanPropType, varName, _getterChain[i].CodegenUnderlyingGet(
                    Ref(lastName), context));
                lastName = varName;
                if (!exists)
                {
                    block.IfRefNullReturnNull(lastName);
                }
                else
                {
                    block.IfRefNullReturnFalse(lastName);
                }
            }
            if (!exists)
            {
                return block.MethodReturn(_getterChain[_getterChain.Length - 1].CodegenUnderlyingGet(
                    Ref(lastName), context));
            }
            else
            {
                return block.MethodReturn(_getterChain[_getterChain.Length - 1].CodegenUnderlyingExists(
                    Ref(lastName), context));
            }
        }
    }
} // end of namespace