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
using com.espertech.esper.events.arr;

namespace com.espertech.esper.events.map
{
    public class MapNestedEntryPropertyGetterArrayObjectArray : MapNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter _getter;
        private readonly int _index;

        public MapNestedEntryPropertyGetterArrayObjectArray(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService, int index, ObjectArrayEventPropertyGetter getter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _index = index;
            _getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArray(value, _index, _getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayFragment(value, _index, _getter,
                FragmentType, EventAdapterService);
        }

        public override ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayCodegen(_index, _getter, name, context,
                GetType());
        }

        public override ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression name,
            ICodegenContext context)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayFragmentCodegen(_index, _getter, name,
                context, GetType());
        }
    }
} // end of namespace