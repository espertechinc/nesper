///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableCompileTimeUtil
    {
        public static StreamTypeServiceImpl StreamTypeFromTableColumn(EventType containedEventType)
        {
            return new StreamTypeServiceImpl(containedEventType, containedEventType.Name, false);
        }

        public static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> CheckTableNameGetLibFunc(
            TableCompileTimeResolver tableService,
            ImportServiceCompileTime importService,
            LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge>
                plugInAggregations,
            string classIdent,
            IList<ExprChainedSpec> chain)
        {
            int index = StringValue.UnescapedIndexOfDot(classIdent);

            // handle special case "table.keys()" function
            if (index == -1) {
                TableMetaData tableX = tableService.Resolve(classIdent);
                if (tableX == null) {
                    return null; // not a table
                }

                string funcName = chain[1].Name;
                if (funcName.Equals("keys", StringComparison.InvariantCultureIgnoreCase)) {
                    var subchain = chain.SubList(2, chain.Count);
                    var node = new ExprTableAccessNodeKeys(tableX.TableName);
                    return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(node, subchain);
                }

                throw new ValidationException(
                    "Invalid use of table '" + classIdent + "', unrecognized use of function '" + funcName +
                    "', expected 'keys()'");
            }

            // Handle "table.property" (without the variable[...] syntax since this is ungrouped use)
            string tableName = StringValue.UnescapeDot(classIdent.Substring(0, index));
            TableMetaData table = tableService.Resolve(tableName);
            if (table == null) {
                return null;
            }

            // this is a table access expression
            var sub = classIdent.Substring(index + 1);
            return HandleTableAccessNode(importService, plugInAggregations, table.TableName, sub, chain);
        }

        public static Pair<ExprNode, IList<ExprChainedSpec>> GetTableNodeChainable(
            StreamTypeService streamTypeService,
            IList<ExprChainedSpec> chainSpec,
            ImportServiceCompileTime importService,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            chainSpec = new List<ExprChainedSpec>(chainSpec);

            string unresolvedPropertyName = chainSpec[0].Name;
            var col = FindTableColumnMayByPrefixed(streamTypeService, unresolvedPropertyName, tableCompileTimeResolver);
            if (col == null) {
                return null;
            }

            var pair = col.Pair;
            if (pair.Column is TableMetadataColumnAggregation) {
                var agg = (TableMetadataColumnAggregation) pair.Column;

                if (chainSpec.Count > 1) {
                    string candidateAccessor = chainSpec[1].Name;
                    var exprNode = (ExprAggregateNodeBase) ASTAggregationHelper.TryResolveAsAggregation(
                        importService, false, candidateAccessor,
                        new LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction,
                            AggregationMultiFunctionForge>());
                    if (exprNode != null) {
                        ExprNode nodeX = new ExprTableIdentNodeSubpropAccessor(
                            pair.StreamNum, col.OptionalStreamName, pair.TableMetadata, agg, exprNode);
                        exprNode.AddChildNodes(chainSpec[1].Parameters);
                        chainSpec.RemoveAt(0);
                        chainSpec.RemoveAt(0);
                        return new Pair<ExprNode, IList<ExprChainedSpec>>(nodeX, chainSpec);
                    }
                }

                Type returnType = pair.TableMetadata.PublicEventType.GetPropertyType(pair.Column.ColumnName);
                var node = new ExprTableIdentNode(
                    pair.TableMetadata, null, unresolvedPropertyName, returnType, pair.StreamNum, agg.Column);
                chainSpec.RemoveAt(0);
                return new Pair<ExprNode, IList<ExprChainedSpec>>(node, chainSpec);
            }

            return null;
        }

        public static ExprTableIdentNode GetTableIdentNode(
            StreamTypeService streamTypeService, string unresolvedPropertyName, string streamOrPropertyName,
            TableCompileTimeResolver resolver)
        {
            var propertyPrefixed = unresolvedPropertyName;
            if (streamOrPropertyName != null) {
                propertyPrefixed = streamOrPropertyName + "." + unresolvedPropertyName;
            }

            var col = FindTableColumnMayByPrefixed(streamTypeService, propertyPrefixed, resolver);
            if (col == null) {
                return null;
            }

            var pair = col.Pair;
            if (pair.Column is TableMetadataColumnAggregation) {
                var agg = (TableMetadataColumnAggregation) pair.Column;
                Type resultType = pair.TableMetadata.PublicEventType.GetPropertyType(agg.ColumnName);
                return new ExprTableIdentNode(
                    pair.TableMetadata, streamOrPropertyName, unresolvedPropertyName, resultType, pair.StreamNum,
                    agg.Column);
            }

            return null;
        }

        public static Pair<ExprTableAccessNode, ExprDotNode> MapPropertyToTableNested(
            TableCompileTimeResolver resolver, string stream, string subproperty)
        {
            TableMetaData table = resolver.Resolve(stream);
            int? indexIfIndexed = null;
            if (table == null) {
                // try indexed property
                var pair = MapPropertyToTable(stream, resolver);
                if (pair == null) {
                    return null;
                }

                table = pair.Second;
                indexIfIndexed = pair.First.Index;
            }

            if (table.IsKeyed && indexIfIndexed == null) {
                return null;
            }

            if (!table.IsKeyed && indexIfIndexed != null) {
                return null;
            }

            int index = StringValue.UnescapedIndexOfDot(subproperty);
            if (index == -1) {
                var tableNodeX = new ExprTableAccessNodeSubprop(table.TableName, subproperty);
                if (indexIfIndexed != null) {
                    tableNodeX.AddChildNode(new ExprConstantNodeImpl(indexIfIndexed));
                }

                return new Pair<ExprTableAccessNode, ExprDotNode>(tableNodeX, null);
            }

            // we have a nested subproperty such as "tablename.subproperty.abc"
            IList<ExprChainedSpec> chainedSpecs = new List<ExprChainedSpec>(1);
            chainedSpecs.Add(
                new ExprChainedSpec(subproperty.Substring(index + 1), Collections.GetEmptyList<ExprNode>(), true));
            var tableNode = new ExprTableAccessNodeSubprop(table.TableName, subproperty.Substring(0, index));
            if (indexIfIndexed != null) {
                tableNode.AddChildNode(new ExprConstantNodeImpl(indexIfIndexed));
            }

            ExprDotNode dotNode = new ExprDotNodeImpl(chainedSpecs, false, false);
            dotNode.AddChildNode(tableNode);
            return new Pair<ExprTableAccessNode, ExprDotNode>(tableNode, dotNode);
        }

        public static Pair<ExprTableAccessNode, IList<ExprChainedSpec>> HandleTableAccessNode(
            ImportServiceCompileTime importService,
            LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge>
                plugInAggregations,
            string tableName,
            string sub,
            IList<ExprChainedSpec> chain)
        {
            ExprTableAccessNode node = new ExprTableAccessNodeSubprop(tableName, sub);
            var subchain = chain.SubList(1, chain.Count);

            string candidateAccessor = subchain[0].Name;
            var exprNode = (ExprAggregateNodeBase) ASTAggregationHelper.TryResolveAsAggregation(
                importService, false, candidateAccessor, plugInAggregations);
            if (exprNode != null) {
                node = new ExprTableAccessNodeSubpropAccessor(tableName, sub, exprNode);
                exprNode.AddChildNodes(subchain[0].Parameters);
                subchain.RemoveAt(0);
            }

            return new Pair<ExprTableAccessNode, IList<ExprChainedSpec>>(node, subchain);
        }

        private static StreamTableColWStreamName FindTableColumnMayByPrefixed(
            StreamTypeService streamTypeService, string streamAndPropName, TableCompileTimeResolver resolver)
        {
            var indexDot = streamAndPropName.IndexOf(".");
            if (indexDot == -1) {
                var pair = FindTableColumnAcrossStreams(streamTypeService, streamAndPropName, resolver);
                if (pair != null) {
                    return new StreamTableColWStreamName(pair, null);
                }
            }
            else {
                var streamName = streamAndPropName.Substring(0, indexDot);
                var colName = streamAndPropName.Substring(indexDot + 1);
                int streamNum = streamTypeService.GetStreamNumForStreamName(streamName);
                if (streamNum == -1) {
                    return null;
                }

                var pair = FindTableColumnForType(
                    streamNum, streamTypeService.EventTypes[streamNum], colName, resolver);
                if (pair != null) {
                    return new StreamTableColWStreamName(pair, streamName);
                }
            }

            return null;
        }

        private static StreamTableColPair FindTableColumnAcrossStreams(
            StreamTypeService streamTypeService, string columnName, TableCompileTimeResolver resolver)
        {
            StreamTableColPair found = null;
            for (var i = 0; i < streamTypeService.EventTypes.Length; i++) {
                EventType type = streamTypeService.EventTypes[i];
                if (type == null) {
                    continue;
                }

                var pair = FindTableColumnForType(i, type, columnName, resolver);
                if (pair == null) {
                    continue;
                }

                if (found != null) {
                    if (streamTypeService.IsStreamZeroUnambigous && found.StreamNum == 0) {
                        continue;
                    }

                    throw new ExprValidationException(
                        "Ambiguous table column '" + columnName + "' should be prefixed by a stream name");
                }

                found = pair;
            }

            return found;
        }

        private static StreamTableColPair FindTableColumnForType(
            int streamNum, EventType type, string columnName, TableCompileTimeResolver resolver)
        {
            TableMetaData tableMetadata = resolver.ResolveTableFromEventType(type);
            if (tableMetadata != null) {
                TableMetadataColumn column = tableMetadata.Columns.Get(columnName);
                if (column != null) {
                    return new StreamTableColPair(streamNum, column, tableMetadata);
                }
            }

            return null;
        }

        /// <summary>
        ///     Handle property "table" or "table[key]" where key is an integer and therefore can be a regular property
        /// </summary>
        /// <param name="propertyName">property</param>
        /// <param name="resolver">resolver</param>
        /// <returns>expression null or node</returns>
        public static ExprTableAccessNode MapPropertyToTableUnnested(
            string propertyName, TableCompileTimeResolver resolver)
        {
            // try regular property
            TableMetaData table = resolver.Resolve(propertyName);
            if (table != null) {
                return new ExprTableAccessNodeTopLevel(table.TableName);
            }

            // try indexed property
            var pair = MapPropertyToTable(propertyName, resolver);
            if (pair == null) {
                return null;
            }

            ExprTableAccessNode tableNode = new ExprTableAccessNodeTopLevel(pair.Second.TableName);
            tableNode.AddChildNode(new ExprConstantNodeImpl(pair.First.Index));
            return tableNode;
        }

        private static Pair<IndexedProperty, TableMetaData> MapPropertyToTable(
            string propertyName, TableCompileTimeResolver resolver)
        {
            try {
                Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (!(property is IndexedProperty)) {
                    return null;
                }

                var name = property.PropertyNameAtomic;
                TableMetaData table = resolver.Resolve(name);
                if (table == null) {
                    return null;
                }

                return new Pair<IndexedProperty, TableMetaData>((IndexedProperty) property, table);
            }
            catch (PropertyAccessException ex) {
                // possible
            }

            return null;
        }

        public class StreamTableColPair
        {
            public StreamTableColPair(int streamNum, TableMetadataColumn column, TableMetaData tableMetadata)
            {
                StreamNum = streamNum;
                Column = column;
                TableMetadata = tableMetadata;
            }

            public int StreamNum { get; }

            public TableMetadataColumn Column { get; }

            public TableMetaData TableMetadata { get; }
        }

        public class StreamTableColWStreamName
        {
            public StreamTableColWStreamName(StreamTableColPair pair, string optionalStreamName)
            {
                Pair = pair;
                OptionalStreamName = optionalStreamName;
            }

            public StreamTableColPair Pair { get; }

            public string OptionalStreamName { get; }
        }
    }
} // end of namespace