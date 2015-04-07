///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.util
{
    public class QueryPlanIndexHookUtil
    {
        public static QueryPlanIndexHook GetHook(Attribute[] annotations)
        {
            try
            {
                return (QueryPlanIndexHook) TypeHelper.GetAnnotationHook(
                        annotations, HookType.INTERNAL_QUERY_PLAN, typeof (QueryPlanIndexHook), null);
            }
            catch (ExprValidationException e)
            {
                throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_QUERY_PLAN);
            }
        }
    }
}