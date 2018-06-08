///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportExprNodeUtil
    {
        public static void Validate(ExprNode node)
        {
            node.Validate(SupportExprValidationContextFactory.MakeEmpty(
                SupportContainer.Instance));
        }
    }
}
