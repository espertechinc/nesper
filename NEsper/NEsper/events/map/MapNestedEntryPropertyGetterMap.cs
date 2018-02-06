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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterMap : MapNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter _mapGetter;

        public MapNestedEntryPropertyGetterMap(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService, MapEventPropertyGetter mapGetter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _mapGetter = mapGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is Map))
            {
                if (value is EventBean) return _mapGetter.Get((EventBean) value);
                return null;
            }

            return _mapGetter.GetMap((IDictionary<string, object>) value);
        }

        private string HandleNestedValueCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(object), "value", GetType())
                .IfNotInstanceOf("value", typeof(Map))
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(_mapGetter.CodegenEventBeanGet(Ref("bean"), context))
                .BlockReturn(ConstantNull())
                .DeclareVarWCast(typeof(Map), "map", "value")
                .MethodReturn(_mapGetter.CodegenUnderlyingGet(Ref("map"), context));
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is Map))
            {
                if (value is EventBean) return _mapGetter.GetFragment((EventBean) value);
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = EventAdapterService.AdapterForTypedMap((IDictionary<string, object>) value, FragmentType);
            return _mapGetter.GetFragment(eventBean);
        }

        private string HandleNestedValueFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(object), "value", GetType())
                .IfNotInstanceOf("value", typeof(Map))
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(_mapGetter.CodegenEventBeanFragment(Ref("bean"), context))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    _mapGetter.CodegenUnderlyingFragment(
                        Cast(typeof(Map), Ref("value")), context));
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context)
        {
            return LocalMethod(HandleNestedValueCodegen(context), name);
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression name,
            ICodegenContext context)
        {
            return LocalMethod(HandleNestedValueFragmentCodegen(context), name);
        }
    }
} // end of namespace