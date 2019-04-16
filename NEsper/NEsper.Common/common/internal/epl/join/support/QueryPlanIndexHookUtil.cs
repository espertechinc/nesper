///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public class QueryPlanIndexHookUtil
    {
        public static QueryPlanIndexHook GetHook(
            Attribute[] annotations,
            ImportService importService)
        {
            try {
                return (QueryPlanIndexHook) ImportUtil.GetAnnotationHook(
                    annotations, HookType.INTERNAL_QUERY_PLAN, typeof(QueryPlanIndexHook), importService);
            }
            catch (ExprValidationException e) {
                throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_QUERY_PLAN);
            }
        }
    }
} // end of namespace