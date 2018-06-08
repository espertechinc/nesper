///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.map;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterArrayMap : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly int _index;
        private readonly MapEventPropertyGetter _getter;

        public ObjectArrayNestedEntryPropertyGetterArrayMap(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService, int index, MapEventPropertyGetter getter)
            : base(propertyIndex, fragmentType, eventAdapterService)
        {
            _index = index;
            _getter = getter;
        }

        public override Object HandleNestedValue(Object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, _index, _getter);
        }

        public override Object HandleNestedValueFragment(Object value)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragment(value, _index, _getter, EventAdapterService, FragmentType);
        }

        public override bool HandleNestedValueExists(Object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExists(value, _index, _getter);
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapCode(_index, _getter, refName, context, GetType());
        }

        public override ICodegenExpression HandleNestedValueExistsCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExistsCode(_index, _getter, refName, context, EventAdapterService, FragmentType, this.GetType());
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression refName, ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragmentCode(_index, _getter, refName, context, EventAdapterService, FragmentType, this.GetType());
        }
    }
} // end of namespace