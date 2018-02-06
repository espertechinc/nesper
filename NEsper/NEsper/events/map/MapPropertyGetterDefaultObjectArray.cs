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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    /// <summary>Getter for map entry.</summary>
    public class MapPropertyGetterDefaultObjectArray : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultObjectArray(string propertyName, EventType fragmentEventType,
            EventAdapterService eventAdapterService)
            : base(propertyName, fragmentEventType, eventAdapterService)
        {
        }

        protected override object HandleCreateFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNCreateFragmentObjectArray(value, FragmentEventType,
                EventAdapterService);
        }

        protected override ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value,
            ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), EventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), FragmentEventType);
            return StaticMethod(typeof(BaseNestableEventUtil), "HandleBNCreateFragmentObjectArray",
                value, Ref(mType.MemberName), Ref(mSvc.MemberName));
        }
    }
} // end of namespace