///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;


namespace com.espertech.esper.support.epl
{
    public class SupportOuterJoinDescFactory
    {
        public static OuterJoinDesc MakeDesc(String propOne, String streamOne, String propTwo, String streamTwo, OuterJoinType type)
        {
            ExprIdentNode identNodeOne = new ExprIdentNodeImpl(propOne, streamOne);
            ExprIdentNode identNodeTwo = new ExprIdentNodeImpl(propTwo, streamTwo);
    
            ExprValidationContext context = ExprValidationContextFactory.Make(new SupportStreamTypeSvc3Stream());
            identNodeOne.Validate(context);
            identNodeTwo.Validate(context);
            OuterJoinDesc desc = new OuterJoinDesc(type, identNodeOne, identNodeTwo, null, null);
    
            return desc;
        }
    }
}
