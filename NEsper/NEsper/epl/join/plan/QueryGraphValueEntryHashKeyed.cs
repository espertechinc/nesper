///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public abstract class QueryGraphValueEntryHashKeyed : QueryGraphValueEntry
    {
        protected QueryGraphValueEntryHashKeyed(ExprNode keyExpr)
        {
            KeyExpr = keyExpr;
        }

        public ExprNode KeyExpr { get; private set; }

        public abstract string ToQueryPlan();

        public static string ToQueryPlan(IList<QueryGraphValueEntryHashKeyed> keyProperties)
        {
            StringWriter writer = new StringWriter();
            String delimiter = "";
            foreach (QueryGraphValueEntryHashKeyed item in keyProperties)
            {
                writer.Write(delimiter);
                writer.Write(item.ToQueryPlan());
                delimiter = ", ";
            }
            return writer.ToString();
        }
    }
}
