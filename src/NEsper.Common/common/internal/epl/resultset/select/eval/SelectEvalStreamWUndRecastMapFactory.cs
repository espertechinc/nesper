///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	public partial class SelectEvalStreamWUndRecastMapFactory
	{

		public static SelectExprProcessorForge Make(
			EventType[] eventTypes,
			SelectExprForgeContext selectExprForgeContext,
			int streamNumber,
			EventType targetType,
			ExprNode[] exprNodes,
			ImportServiceCompileTime importService,
			string statementName)
		{
			var mapResultType = (MapEventType)targetType;
			var mapStreamType = (MapEventType)eventTypes[streamNumber];

			// (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
			var typeSameMssage = BaseNestableEventType.IsDeepEqualsProperties(
				mapResultType.Name,
				mapResultType.Types,
				mapStreamType.Types,
				false);
			if (typeSameMssage == null && selectExprForgeContext.ExprForges.Length == 0) {
				return new MapInsertProcessorSimpleRepackage(selectExprForgeContext, streamNumber, targetType);
			}

			// (B) not completely assignable: find matching properties
			var writables = EventTypeUtility.GetWriteableProperties(mapResultType, true, false);
			IList<Item> items = new List<Item>();
			IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

			// find the properties coming from the providing source stream
			var count = 0;
			foreach (var writeable in writables) {
				var propertyName = writeable.PropertyName;

				if (mapStreamType.Types.ContainsKey(propertyName)) {
					var setOneType = mapResultType.Types.Get(propertyName);
					var setOneTypeFound = mapResultType.Types.ContainsKey(propertyName);
					var setTwoType = mapStreamType.Types.Get(propertyName);
					var message = BaseNestableEventUtil.ComparePropType(
						propertyName,
						setOneType,
						setOneTypeFound,
						setTwoType,
						mapResultType.Name);
					if (message != null) {
						throw new ExprValidationException(message.Message, message);
					}

					items.Add(new Item(count, propertyName, null, null));
					written.Add(writeable);
					count++;
				}
			}

			// find the properties coming from the expressions of the select clause
			for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
				var columnName = selectExprForgeContext.ColumnNames[i];
				var exprNode = exprNodes[i];

				var writable = FindWritable(columnName, writables);
				if (writable == null) {
					throw new ExprValidationException(
						"Failed to find column '" + columnName + "' in target type '" + mapResultType.Name + "'");
				}

				try {
					var widener = TypeWidenerFactory.GetCheckPropertyAssignType(
						ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
						exprNode.Forge.EvaluationType,
						writable.PropertyType,
						columnName,
						false,
						null,
						statementName);
					items.Add(new Item(count, null, exprNode.Forge, widener));
					written.Add(writable);
					count++;
				}
				catch (TypeWidenerException ex) {
					throw new ExprValidationException(ex.Message, ex);
				}
			}

			// make manufacturer
			var itemsArr = items.ToArray();
			EventBeanManufacturerForge manufacturer;
			try {
				manufacturer = EventTypeUtility.GetManufacturer(
					mapResultType,
					written.ToArray(),
					importService,
					true,
					null);
			}
			catch (EventBeanManufactureException e) {
				throw new ExprValidationException("Failed to write to type: " + e.Message, e);
			}

			return new MapInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
		}

		private static WriteablePropertyDescriptor FindWritable(
			string columnName,
			ISet<WriteablePropertyDescriptor> writables)
		{
			foreach (var writable in writables) {
				if (writable.PropertyName.Equals(columnName)) {
					return writable;
				}
			}

			return null;
		}
	}
} // end of namespace
