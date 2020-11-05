///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapNestedEntryPropertyGetterPropertyProvidedDynamic : MapNestedEntryPropertyGetterBase
    {
        private readonly EventPropertyGetter _nestedGetter;

        public MapNestedEntryPropertyGetterPropertyProvidedDynamic(
            string propertyMap,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventPropertyGetter nestedGetter)
            : base(propertyMap, fragmentType, eventBeanTypedEventFactory)
        {
            this._nestedGetter = nestedGetter;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public override object HandleNestedValue(object value)
        {
            if ((value is IDictionary<string, object> mapValue) &&
                (_nestedGetter is MapEventPropertyGetter mapEventPropertyGetter)) {
                return mapEventPropertyGetter.GetMap(mapValue);
            }

            return null;
        }

        public override bool HandleNestedValueExists(object value)
        {
            if ((value is IDictionary<string, object> mapValue) &&
                (_nestedGetter is MapEventPropertyGetter mapEventPropertyGetter)) {
                return mapEventPropertyGetter.IsMapExistsProperty(mapValue);
            }

            return false;
        }

        private CodegenMethod HandleNestedValueCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value")
                .Block
                .IfRefNotTypeReturnConst(
                    "value",
                    typeof(IDictionary<string, object>),
                    null);
            if (_nestedGetter is MapEventPropertyGetter eventPropertyGetter) {
                return block.MethodReturn(
                    eventPropertyGetter.UnderlyingGetCodegen(
                        Cast(typeof(IDictionary<string, object>), Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
            }

            return block.MethodReturn(ConstantNull());
        }

        private CodegenMethod HandleNestedValueExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenBlock block = codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value")
                .Block
                .IfRefNotTypeReturnConst("value", typeof(IDictionary<string, object>), false);
            if (_nestedGetter is MapEventPropertyGetter eventPropertyGetter) {
                return block
                    .MethodReturn(
                        eventPropertyGetter.UnderlyingExistsCodegen(
                            Cast<IDictionary<string, object>>(Ref("value")),
                            codegenMethodScope,
                            codegenClassScope));
            }

            return block.MethodReturn(ConstantFalse());
        }

        private bool IsExistsProperty(IDictionary<string, object> map)
        {
            var value = map.Get(propertyMap);
            if ((value is IDictionary<string, object> mapValue) &&
                (_nestedGetter is MapEventPropertyGetter eventPropertyGetter)) {
                return eventPropertyGetter.IsMapExistsProperty(mapValue);
            }

            return false;
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(propertyMap)))
                .IfRefNullReturnFalse("value")
                .IfRefNotTypeReturnConst("value", typeof(IDictionary<string, object>), false);
            if (_nestedGetter is MapEventPropertyGetter eventPropertyGetter) {
                return block.MethodReturn(
                    eventPropertyGetter.UnderlyingExistsCodegen(
                        Cast(typeof(IDictionary<string, object>), Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
            }

            return block.MethodReturn(ConstantFalse());
        }

        public override object HandleNestedValueFragment(object value)
        {
            return null;
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression valueExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueCodegen(codegenMethodScope, codegenClassScope), valueExpression);
        }

        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression valueExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueExistsCodegen(codegenMethodScope, codegenClassScope), valueExpression);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }
    }
} // end of namespace