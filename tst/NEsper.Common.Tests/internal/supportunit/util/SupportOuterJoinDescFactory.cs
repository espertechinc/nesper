///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportOuterJoinDescFactory
    {
        public static OuterJoinDesc MakeDesc(
            IContainer container,
            string propOne,
            string streamOne,
            string propTwo,
            string streamTwo,
            OuterJoinType type)
        {
            ExprIdentNode identNodeOne = new ExprIdentNodeImpl(propOne, streamOne);
            ExprIdentNode identNodeTwo = new ExprIdentNodeImpl(propTwo, streamTwo);

            var context = SupportExprValidationContextFactory.Make(
                container,
                new SupportStreamTypeSvc3Stream(SupportEventTypeFactory.GetInstance(container)));
            identNodeOne.Validate(context);
            identNodeTwo.Validate(context);
            var desc = new OuterJoinDesc(type, identNodeOne, identNodeTwo, null, null);

            return desc;
        }
    }
} // end of namespace
