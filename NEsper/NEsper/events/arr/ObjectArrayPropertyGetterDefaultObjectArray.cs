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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Getter for map entry.
    /// </summary>
    /// <seealso cref="com.espertech.esper.events.arr.ObjectArrayPropertyGetterDefaultBase" />
    public class ObjectArrayPropertyGetterDefaultObjectArray : ObjectArrayPropertyGetterDefaultBase
    {
        public ObjectArrayPropertyGetterDefaultObjectArray(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
            : base(propertyIndex, fragmentEventType, eventAdapterService)
        {
        }

        protected override Object HandleCreateFragment(Object value)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }
            return BaseNestableEventUtil.HandleBNCreateFragmentObjectArray(value, _fragmentEventType, _eventAdapterService);
        }

        protected override ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value, ICodegenContext context)
        {
            if (_fragmentEventType == null)
            {
                return ConstantNull();
            }
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentEventType);
            return StaticMethod(typeof(BaseNestableEventUtil), "HandleBNCreateFragmentObjectArray", value, 
                Ref(mType.MemberName),
                Ref(mSvc.MemberName));
        }
    }
} // end of namespace