///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.plugin;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    public class ASTTableExprHelper
    {
        /// <summary>
        /// Add reference to table to statement spec.
        /// </summary>
        public static void AddTableExpressionReference(StatementSpecRaw statementSpec, ExprTableAccessNode tableNode)
        {
            if (statementSpec.TableExpressions == null) {
                statementSpec.TableExpressions = new HashSet<ExprTableAccessNode>();
            }
            statementSpec.TableExpressions.Add(tableNode);
        }
    
        public static void AddTableExpressionReference(StatementSpecRaw statementSpec, ICollection<ExprTableAccessNode> tableNodes)
        {
            if (tableNodes == null || tableNodes.IsEmpty()) {
                return;
            }
            if (statementSpec.TableExpressions == null) {
                statementSpec.TableExpressions = new HashSet<ExprTableAccessNode>();
            }
            statementSpec.TableExpressions.AddAll(tableNodes);
        }
    
        /// <summary>
        /// Resolve "table.property", not chainable
        /// </summary>
        public static Pair<ExprTableAccessNode, ExprDotNode> CheckTableNameGetExprForSubproperty(TableService tableService, string tableName, string subproperty)
        {
            var metadata = tableService.GetTableMetadata(tableName);
            if (metadata == null) {
                return null;
            }
    
            var index = ASTUtil.UnescapedIndexOfDot(subproperty);
            if (index == -1) {
                if (metadata.KeyTypes.Length > 0) {
                    return null;
                }
                var tableNodeX = new ExprTableAccessNodeSubprop(tableName, subproperty);
                return new Pair<ExprTableAccessNode, ExprDotNode>(tableNodeX, null);
            }
    
            // we have a nested subproperty such as "tablename.subproperty.abc"
            IList<ExprChainedSpec> chainedSpecs = new List<ExprChainedSpec>(1);
            chainedSpecs.Add(new ExprChainedSpec(subproperty.Substring(index + 1), Collections.GetEmptyList<ExprNode>(), true));
            var tableNode = new ExprTableAccessNodeSubprop(tableName, subproperty.Substring(0, index));
            var dotNode = new ExprDotNode(chainedSpecs, false, false);
            dotNode.AddChildNode(tableNode);
            return new Pair<ExprTableAccessNode, ExprDotNode>(tableNode, dotNode);
        }
    
        /// <summary>
        /// Resolve "table" and "table.property" when nested-property, not chainable
        /// </summary>
        public static ExprTableAccessNode CheckTableNameGetExprForProperty(TableService tableService, string propertyName) {
    
            // handle "var_name" alone, without chained, like an simple event property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                if (tableService.GetTableMetadata(propertyName) != null) {
                    return new ExprTableAccessNodeTopLevel(propertyName);
                }
                return null;
            }
    
            // handle "var_name.column", without chained, like a nested event property
            var tableName = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            if (tableService.GetTableMetadata(tableName) == null) {
                return null;
            }
    
            // it is a tables's subproperty
            var sub = propertyName.Substring(index + 1);
            return new ExprTableAccessNodeSubprop(tableName, sub);
        }
    
        public static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> CheckTableNameGetLibFunc(
                TableService tableService,
                EngineImportService engineImportService,
                LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> plugInAggregations,
                string engineURI,
                string classIdent,
                IList<ExprChainedSpec> chain) {
    
            var index = ASTUtil.UnescapedIndexOfDot(classIdent);
    
            // handle special case "table.keys()" function
            if (index == -1) {
                if (tableService.GetTableMetadata(classIdent) == null) {
                    return null; // not a table
                }
                var funcName = chain[1].Name;
                if (funcName.ToLower().Equals("keys")) {
                    IList<ExprChainedSpec> subchain = chain.SubList(2, chain.Count);
                    var node = new ExprTableAccessNodeKeys(classIdent);
                    return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(node, subchain);
                }
                else {
                    throw ASTWalkException.From("Invalid use of variable '" + classIdent + "', unrecognized use of function '" + funcName + "', expected 'keys()'");
                }
            }
    
            // Handle "table.property" (without the variable[...] syntax since this is ungrouped use)
            var tableName = ASTUtil.UnescapeDot(classIdent.Substring(0, index));
            if (tableService.GetTableMetadata(tableName) == null) {
                return null;
            }
    
            // this is a table access expression
            var sub = classIdent.Substring(index + 1);
            return HandleTable(engineImportService, plugInAggregations, engineURI, tableName, sub, chain);
        }
    
        /// <summary>
        /// Handle "variable[...].xxxxx......"
        /// </summary>
        public static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> GetTableExprChainable(
                EngineImportService engineImportService,
                LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> plugInAggregations,
                string engineURI,
                string tableName,
                IList<ExprChainedSpec> chain)
        {
    
            // handle just "variable[...].sub"
            var subpropName = chain[0].Name;
            if (chain.Count == 1) {
                chain.RemoveAt(0);
                var tableNode = new ExprTableAccessNodeSubprop(tableName, subpropName);
                return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(tableNode, chain);
            }
    
            // we have a chain "variable[...].sub.xyz"
            return HandleTable(engineImportService, plugInAggregations, engineURI,
                    tableName, subpropName, chain);
        }
    
        private static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> HandleTable(
                EngineImportService engineImportService,
                LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> plugInAggregations,
                string engineURI,
                string tableName,
                string sub,
                IList<ExprChainedSpec> chain) {
    
            ExprTableAccessNode node = new ExprTableAccessNodeSubprop(tableName, sub);
            IList<ExprChainedSpec> subchain = chain.SubList(1, chain.Count);
    
            var candidateAccessor = subchain[0].Name;
            var exprNode = (ExprAggregateNodeBase) ASTAggregationHelper.TryResolveAsAggregation(engineImportService, false, candidateAccessor, plugInAggregations, engineURI);
            if (exprNode != null) {
                node = new ExprTableAccessNodeSubpropAccessor(tableName, sub, exprNode);
                exprNode.AddChildNodes(subchain[0].Parameters);
                subchain.RemoveAt(0);
            }
    
            return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(node, subchain);
    
        }
    }
}
