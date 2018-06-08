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
using com.espertech.esper.codegen.model.blocks;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.map;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterMap : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter _mapGetter;

        public ObjectArrayNestedEntryPropertyGetterMap(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService, MapEventPropertyGetter mapGetter)
            : base(propertyIndex, fragmentType, eventAdapterService)
        {
            _mapGetter = mapGetter;
        }

        public override Object HandleNestedValue(Object value)
        {
            if (value == null) {
                return null;
            }
            else if (value is Map) {
                return _mapGetter.GetMap((Map) value);
            }
            else if (value is EventBean) {
                return _mapGetter.Get((EventBean) value);
            }
            else if (value.GetType().IsGenericStringDictionary()) {
                return _mapGetter.GetMap(value.AsStringDictionary());
            }

            return null;
        }

        public override Object HandleNestedValueFragment(Object value)
        {
            if (!(value is Map))
            {
                if (value is EventBean)
                {
                    return _mapGetter.GetFragment((EventBean)value);
                }
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = EventAdapterService.AdapterForTypedMap((Map) value, FragmentType);
            return _mapGetter.GetFragment(eventBean);
        }

        public override bool HandleNestedValueExists(Object value)
        {
            if (value is Map valueAsMap)
                return _mapGetter.IsMapExistsProperty(valueAsMap);
            if (value is EventBean valueAsEventBean)
                return _mapGetter.IsExistsProperty(valueAsEventBean);

            return false;
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return LocalMethod(CodegenBlockPropertyBeanOrUnd.From(context, typeof(Map), _mapGetter, CodegenBlockPropertyBeanOrUnd.AccessType.GET, this.GetType()), refName);
        }

        public override ICodegenExpression HandleNestedValueExistsCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return LocalMethod(CodegenBlockPropertyBeanOrUnd.From(context, typeof(Map), _mapGetter, CodegenBlockPropertyBeanOrUnd.AccessType.EXISTS, this.GetType()), refName);
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return LocalMethod(CodegenBlockPropertyBeanOrUnd.From(context, typeof(Map), _mapGetter, CodegenBlockPropertyBeanOrUnd.AccessType.FRAGMENT, this.GetType()), refName);
        }
    }
} // end of namespace