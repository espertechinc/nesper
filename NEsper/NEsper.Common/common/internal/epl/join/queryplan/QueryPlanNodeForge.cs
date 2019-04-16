///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    /// Specification node for a query execution plan to be extended by specific execution specification nodes.
    /// </summary>
    public abstract class QueryPlanNodeForge : CodegenMakeable<SAIFFInitializeSymbol>
    {
        public abstract void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes);

        public abstract void Accept(QueryPlanNodeForgeVisitor visitor);

        public abstract CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        /// <summary>
        /// Print a long readable format of the query node to the supplied PrintWriter.
        /// </summary>
        /// <param name="writer">is the indentation writer to print to</param>
        protected abstract void Print(IndentWriter writer);

        /// <summary>
        /// Print in readable format the execution plan spec.
        /// </summary>
        /// <param name="planNodeSpecs">plans to print</param>
        /// <returns>readable text with plans</returns>
        public static string Print(QueryPlanNodeForge[] planNodeSpecs)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("QueryPlanNode[]\n");

            for (int i = 0; i < planNodeSpecs.Length; i++) {
                buffer.Append("  node spec " + i + " :\n");

                StringWriter writer = new StringWriter();
                IndentWriter indentWriter = new IndentWriter(writer, 4, 2);

                if (planNodeSpecs[i] != null) {
                    planNodeSpecs[i].Print(indentWriter);
                }
                else {
                    indentWriter.WriteLine("no plan (historical)");
                }

                buffer.Append(writer.ToString());
            }

            return buffer.ToString();
        }
    }
} // end of namespace