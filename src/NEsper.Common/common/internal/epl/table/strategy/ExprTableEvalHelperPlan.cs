///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalHelperPlan
    {
        public static IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> PlanTableAccess(
            ICollection<ExprTableAccessNode> tableAccessNodes)
        {
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges =
                new LinkedHashMap<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge>();

            foreach (var entry in tableAccessNodes) {
                var forge = entry.TableAccessFactoryForge;
                tableAccessForges.Put(entry, forge);
            }

            return tableAccessForges;
        }
    }
} // end of namespace