///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    public class MapNestedEntryPropertyGetterPropertyProvidedDynamic : MapNestedEntryPropertyGetterBase
    {
        private readonly EventPropertyGetter _nestedGetter;

        public MapNestedEntryPropertyGetterPropertyProvidedDynamic(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService, EventPropertyGetter nestedGetter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            this._nestedGetter = nestedGetter;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is Map)) return null;
            if (_nestedGetter is MapEventPropertyGetter)
                return ((MapEventPropertyGetter) _nestedGetter).GetMap((IDictionary<string, object>) value);
            return null;
        }

        private string HandleNestedValueCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(object), typeof(object), "value", GetType())
                .IfRefNotTypeReturnConst("value", typeof(Map), "null");
            if (_nestedGetter is MapEventPropertyGetter)
                return block.MethodReturn(
                    ((MapEventPropertyGetter) _nestedGetter).CodegenUnderlyingGet(
                        Cast(typeof(Map), Ref("value")), context));
            return block.MethodReturn(ConstantNull());
        }

        private bool IsExistsProperty(Map map)
        {
            var value = map.Get(PropertyMap);
            if (value == null || !(value is Map)) return false;
            if (_nestedGetter is MapEventPropertyGetter)
                return ((MapEventPropertyGetter) _nestedGetter).IsMapExistsProperty((Map) value);
            return false;
        }

        private string IsExistsPropertyCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(bool), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(PropertyMap)))
                .IfRefNullReturnFalse("value")
                .IfRefNotTypeReturnConst("value", typeof(Map), false);
            if (_nestedGetter is MapEventPropertyGetter)
                return block.MethodReturn(
                    ((MapEventPropertyGetter) _nestedGetter).CodegenUnderlyingExists(
                        Cast(typeof(Map), Ref("value")), context));
            return block.MethodReturn(ConstantFalse());
        }

        public override object HandleNestedValueFragment(object value)
        {
            return null;
        }

        public override ICodegenExpression HandleNestedValueCodegen(
            ICodegenExpression valueExpression, ICodegenContext context)
        {
            return LocalMethod(HandleNestedValueCodegen(context), valueExpression);
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(
            ICodegenExpression name, ICodegenContext context)
        {
            return ConstantNull();
        }

        public override ICodegenExpression CodegenEventBeanExists(
            ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public override ICodegenExpression CodegenUnderlyingExists(
            ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(IsExistsPropertyCodegen(context), underlyingExpression);
        }
    }
} // end of namespace