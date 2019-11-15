///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTTableExprHelper
    {
        public static ExprTableAccessNode CheckTableNameGetExprForProperty(
            TableCompileTimeResolver tableCompileTimeResolver,
            string propertyName)
        {
            // handle "var_name" alone, without chained, like an simple event property
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                var table = tableCompileTimeResolver.Resolve(propertyName);
                return table == null ? null : new ExprTableAccessNodeTopLevel(table.TableName);
            }

            // handle "var_name.column", without chained, like a nested event property
            var tableName = StringValue.UnescapeDot(propertyName.Substring(0, index));
            var metaData = tableCompileTimeResolver.Resolve(tableName);
            if (metaData == null)
            {
                return null;
            }

            // it is a tables's subproperty
            var sub = propertyName.Substring(index + 1);
            return new ExprTableAccessNodeSubprop(metaData.TableName, sub);
        }

        public static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> GetTableExprChainable(
            ImportServiceCompileTime importService,
            LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge> plugInAggregations,
            string tableName,
            IList<ExprChainedSpec> chain)
        {
            // handle just "variable[...].sub"
            var subpropName = chain[0].Name;
            if (chain.Count == 1)
            {
                chain.RemoveAt(0);
                var tableNode = new ExprTableAccessNodeSubprop(tableName, subpropName);
                return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(tableNode, chain);
            }

            // we have a chain "variable[...].sub.xyz"
            return TableCompileTimeUtil.HandleTableAccessNode(importService, plugInAggregations, tableName, subpropName, chain);
        }
    }
} // end of namespace