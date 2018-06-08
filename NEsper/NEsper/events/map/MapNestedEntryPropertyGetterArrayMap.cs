///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.events.map
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterArrayMap : MapNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter _getter;
        private readonly int _index;

        public MapNestedEntryPropertyGetterArrayMap(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService, int index, MapEventPropertyGetter getter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _index = index;
            _getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, _index, _getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragment(value, _index, _getter,
                EventAdapterService, FragmentType);
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapCode(_index, _getter, name, context, GetType());
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression name,
            ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragmentCode(_index, _getter, name, context,
                EventAdapterService, FragmentType, GetType());
        }
    }
} // end of namespace