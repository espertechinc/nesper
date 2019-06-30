///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportExprNodeUtil
    {
        public static void Validate(IContainer container, ExprNode node)
        {
            try
            {
                node.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }
            catch (ExprValidationException ex)
            {
                throw new EPRuntimeException(ex);
            }
        }
    }
} // end of namespace