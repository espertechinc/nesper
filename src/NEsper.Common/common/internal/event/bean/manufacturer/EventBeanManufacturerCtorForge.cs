///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class EventBeanManufacturerCtorForge : EventBeanManufacturerForge
    {
        private readonly BeanEventType _beanEventType;
        private readonly ConstructorInfo _constructor;

        public EventBeanManufacturerCtorForge(
            ConstructorInfo constructor,
            BeanEventType beanEventType)
        {
            this._constructor = constructor;
            this._beanEventType = beanEventType;
        }

        public CodegenExpression Make(
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var beanType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_beanEventType, EPStatementInitServicesConstants.REF));
            var ctor = StaticMethod(
                typeof(EventBeanManufacturerCtorForge),
                "ResolveConstructor",
                Constant(_constructor.GetParameterTypes()),
                Constant(_constructor.DeclaringType));
            return NewInstance<EventBeanManufacturerCtor>(ctor, beanType, factory);
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerCtor(_constructor, _beanEventType, eventBeanTypedEventFactory);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="classes">classes</param>
        /// <param name="declaring">declaring</param>
        /// <returns>ctor</returns>
        public static ConstructorInfo ResolveConstructor(
            Type[] classes,
            Type declaring)
        {
            try {
                return declaring.GetConstructor(classes);
            }
            catch (Exception) {
                throw new EPException(
                    "Failed to resolve constructor for class " +
                    declaring.GetType() +
                    " params " +
                    classes.RenderAny());
            }
        }
    }
} // end of namespace