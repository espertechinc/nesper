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
    public class MapPropertyGetterDefaultMap : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultMap(string propertyName, EventType fragmentEventType,
            EventAdapterService eventAdapterService)
            : base(propertyName, fragmentEventType, eventAdapterService)
        {
        }

        protected override object HandleCreateFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNCreateFragmentMap(value, FragmentEventType, EventAdapterService);
        }

        protected override ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value,
            ICodegenContext context)
        {
            var mType = context.MakeAddMember(typeof(EventType), FragmentEventType);
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), EventAdapterService);
            return StaticMethod(typeof(BaseNestableEventUtil), "handleBNCreateFragmentMap", value,
                Ref(mType.MemberName), Ref(mSvc.MemberName));
        }
    }
} // end of namespace