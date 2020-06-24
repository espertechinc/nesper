///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.core.AggregationPortableValidationBase;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
	public class TableCompileTimeUtil
	{
		public static StreamTypeServiceImpl StreamTypeFromTableColumn(EventType containedEventType)
		{
			return new StreamTypeServiceImpl(containedEventType, containedEventType.Name, false);
		}

		public static Pair<ExprTableAccessNode, IList<Chainable>> CheckTableNameGetLibFunc(
			TableCompileTimeResolver tableService,
			LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge> plugInAggregations,
			string classIdent,
			IList<Chainable> chain)
		{

			var index = StringValue.UnescapedIndexOfDot(classIdent);

			// handle special case "table.keys()" function
			if (index == -1) {
				var tableX = tableService.Resolve(classIdent);
				if (tableX == null) {
					return null; // not a table
				}

				var funcName = chain[1].GetRootNameOrEmptyString();
				if (funcName.Equals("keys", StringComparison.InvariantCultureIgnoreCase)) {
					var subchain = chain.SubList(2, chain.Count);
					var node = new ExprTableAccessNodeKeys(tableX.TableName);
					return new Pair<ExprTableAccessNode, IList<Chainable>>(node, subchain);
				}
				else {
					throw new ValidationException(
						"Invalid use of table '" + classIdent + "', unrecognized use of function '" + funcName + "', expected 'keys()'");
				}
			}

			// Handle "table.property" (without the variable[...] syntax since this is ungrouped use)
			var tableName = StringValue.UnescapeDot(classIdent.Substring(0, index));
			var table = tableService.Resolve(tableName);
			if (table == null) {
				return null;
			}

			// this is a table access expression
			var sub = classIdent.Substring(index + 1, classIdent.Length);
			return HandleTableAccessNode(plugInAggregations, table.TableName, sub, chain);
		}

		public static Pair<ExprNode, IList<Chainable>> GetTableNodeChainable(
			StreamTypeService streamTypeService,
			IList<Chainable> chainSpec,
			bool allowTableAggReset,
			TableCompileTimeResolver tableCompileTimeResolver)
		{
			chainSpec = new List<Chainable>(chainSpec);

			var unresolvedPropertyName = chainSpec[0].GetRootNameOrEmptyString();
			var tableStreamNum = streamTypeService.GetStreamNumForStreamName(unresolvedPropertyName);
			if (chainSpec.Count == 2 && tableStreamNum != -1) {
				var tableMetadata = tableCompileTimeResolver.ResolveTableFromEventType(streamTypeService.EventTypes[tableStreamNum]);
				if (tableMetadata != null && chainSpec[1].GetRootNameOrEmptyString().Equals("reset", StringComparison.InvariantCultureIgnoreCase)) {
					if (!allowTableAggReset) {
						throw new ExprValidationException(INVALID_TABLE_AGG_RESET);
					}

					if (!chainSpec[1].GetParametersOrEmpty().IsEmpty()) {
						throw new ExprValidationException(INVALID_TABLE_AGG_RESET_PARAMS);
					}

					var node = new ExprTableResetRowAggNode(tableMetadata, tableStreamNum);
					chainSpec.Clear();
					return new Pair<ExprNode, IList<Chainable>>(node, chainSpec);
				}
			}

			var col = FindTableColumnMayByPrefixed(streamTypeService, unresolvedPropertyName, tableCompileTimeResolver);
			if (col == null) {
				return null;
			}

			var pair = col.Pair;
			if (pair.Column is TableMetadataColumnAggregation) {
				var agg = (TableMetadataColumnAggregation) pair.Column;
				var returnType = pair.TableMetadata.PublicEventType.GetPropertyType(pair.Column.ColumnName);
				var node = new ExprTableIdentNode(
					pair.TableMetadata,
					null,
					unresolvedPropertyName,
					returnType,
					pair.StreamNum,
					agg.ColumnName,
					agg.Column);
				chainSpec.RemoveAt(0);
				return new Pair<ExprNode, IList<Chainable>>(node, chainSpec);
			}

			return null;
		}

		public static ExprTableIdentNode GetTableIdentNode(
			StreamTypeService streamTypeService,
			string unresolvedPropertyName,
			string streamOrPropertyName,
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
				var resultType = pair.TableMetadata.PublicEventType.GetPropertyType(agg.ColumnName);
				return new ExprTableIdentNode(
					pair.TableMetadata,
					streamOrPropertyName,
					unresolvedPropertyName,
					resultType,
					pair.StreamNum,
					agg.ColumnName,
					agg.Column);
			}

			return null;
		}

		public static Pair<ExprTableAccessNode, ExprDotNode> MapPropertyToTableNested(
			TableCompileTimeResolver resolver,
			string stream,
			string subproperty)
		{

			var table = resolver.Resolve(stream);
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

			var index = StringValue.UnescapedIndexOfDot(subproperty);
			if (index == -1) {
				var tableNodeX = new ExprTableAccessNodeSubprop(table.TableName, subproperty);
				if (indexIfIndexed != null) {
					tableNodeX.AddChildNode(new ExprConstantNodeImpl(indexIfIndexed));
				}

				return new Pair<ExprTableAccessNode, ExprDotNode>(tableNodeX, null);
			}

			// we have a nested subproperty such as "tablename.subproperty.abc"
			IList<Chainable> chainedSpecs = new List<Chainable>(1);
			chainedSpecs.Add(new ChainableName(subproperty.Substring(index + 1)));
			var tableNode = new ExprTableAccessNodeSubprop(table.TableName, subproperty.Substring(0, index));
			if (indexIfIndexed != null) {
				tableNode.AddChildNode(new ExprConstantNodeImpl(indexIfIndexed));
			}

			ExprDotNode dotNode = new ExprDotNodeImpl(chainedSpecs, false, false);
			dotNode.AddChildNode(tableNode);
			return new Pair<ExprTableAccessNode, ExprDotNode>(tableNode, dotNode);
		}

		public static Pair<ExprTableAccessNode, IList<Chainable>> HandleTableAccessNode(
			LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge> plugInAggregations,
			string tableName,
			string sub,
			IList<Chainable> chain)
		{
			ExprTableAccessNode node = new ExprTableAccessNodeSubprop(tableName, sub);
			var subchain = chain.SubList(1, chain.Count);
			return new Pair<ExprTableAccessNode, IList<Chainable>>(node, subchain);
		}

		private static StreamTableColWStreamName FindTableColumnMayByPrefixed(
			StreamTypeService streamTypeService,
			string streamAndPropName,
			TableCompileTimeResolver resolver)
		{
			var indexDot = streamAndPropName.IndexOf('.');
			if (indexDot == -1) {
				var pair = FindTableColumnAcrossStreams(streamTypeService, streamAndPropName, resolver);
				if (pair != null) {
					return new StreamTableColWStreamName(pair, null);
				}
			}
			else {
				var streamName = streamAndPropName.Substring(0, indexDot);
				var colName = streamAndPropName.Substring(indexDot + 1);
				var streamNum = streamTypeService.GetStreamNumForStreamName(streamName);
				if (streamNum == -1) {
					return null;
				}

				var pair = FindTableColumnForType(streamNum, streamTypeService.EventTypes[streamNum], colName, resolver);
				if (pair != null) {
					return new StreamTableColWStreamName(pair, streamName);
				}
			}

			return null;
		}

		private static StreamTableColPair FindTableColumnAcrossStreams(
			StreamTypeService streamTypeService,
			string columnName,
			TableCompileTimeResolver resolver)
		{
			StreamTableColPair found = null;
			for (var i = 0; i < streamTypeService.EventTypes.Length; i++) {
				var type = streamTypeService.EventTypes[i];
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

					throw new ExprValidationException("Ambiguous table column '" + columnName + "' should be prefixed by a stream name");
				}

				found = pair;
			}

			return found;
		}

		private static StreamTableColPair FindTableColumnForType(
			int streamNum,
			EventType type,
			string columnName,
			TableCompileTimeResolver resolver)
		{
			var tableMetadata = resolver.ResolveTableFromEventType(type);
			if (tableMetadata != null) {
				var column = tableMetadata.Columns.Get(columnName);
				if (column != null) {
					return new StreamTableColPair(streamNum, column, tableMetadata);
				}
			}

			return null;
		}

		/// <summary>
		/// Handle property "table" or "table[key]" where key is an integer and therefore can be a regular property
		/// </summary>
		/// <param name="propertyName">property</param>
		/// <param name="resolver">resolver</param>
		/// <returns>expression null or node</returns>
		public static ExprTableAccessNode MapPropertyToTableUnnested(
			string propertyName,
			TableCompileTimeResolver resolver)
		{
			// try regular property
			var table = resolver.Resolve(propertyName);
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
			string propertyName,
			TableCompileTimeResolver resolver)
		{
			try {
				var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
				if (!(property is IndexedProperty)) {
					return null;
				}

				var name = property.PropertyNameAtomic;
				var table = resolver.Resolve(name);
				if (table == null) {
					return null;
				}

				return new Pair<IndexedProperty, TableMetaData>((IndexedProperty) property, table);
			}
			catch (PropertyAccessException) {
				// possible
			}

			return null;
		}

		internal class StreamTableColPair
		{
			internal StreamTableColPair(
				int streamNum,
				TableMetadataColumn column,
				TableMetaData tableMetadata)
			{
				this.StreamNum = streamNum;
				this.Column = column;
				this.TableMetadata = tableMetadata;
			}

			public int StreamNum { get; }

			public TableMetadataColumn Column { get; }

			public TableMetaData TableMetadata { get; }
		}

		internal class StreamTableColWStreamName
		{
			internal StreamTableColWStreamName(
				StreamTableColPair pair,
				string optionalStreamName)
			{
				Pair = pair;
				OptionalStreamName = optionalStreamName;
			}

			public StreamTableColPair Pair { get; }

			public string OptionalStreamName { get; }
		}
	}
} // end of namespace
