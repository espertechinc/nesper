///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public partial class SelectEvalStreamWUndRecastObjectArrayFactory
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
            var oaResultType = (ObjectArrayEventType)targetType;
            var oaStreamType = (ObjectArrayEventType)eventTypes[streamNumber];

            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            if (oaResultType.IsDeepEqualsConsiderOrder(oaStreamType) && selectExprForgeContext.ExprForges.Length == 0) {
                return new OAInsertProcessorSimpleRepackage(selectExprForgeContext, streamNumber, targetType);
            }

            // (B) not completely assignable: find matching properties
            var writables = EventTypeUtility.GetWriteableProperties(oaResultType, true, false);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

            // find the properties coming from the providing source stream
            foreach (var writeable in writables) {
                var propertyName = writeable.PropertyName;

                if (oaStreamType.PropertiesIndexes.TryGetValue(propertyName, out var indexSource)) {
                    var indexTarget = oaResultType.PropertiesIndexes[propertyName];
                    var setOneType = oaResultType.Types.Get(propertyName);
                    var setOneTypeFound = oaResultType.Types.ContainsKey(propertyName);
                    var setTwoType = oaStreamType.Types.Get(propertyName);
                    var message = BaseNestableEventUtil.ComparePropType(
                        propertyName,
                        setOneType,
                        setOneTypeFound,
                        setTwoType,
                        oaResultType.Name);
                    if (message != null) {
                        throw new ExprValidationException(message.Message, message);
                    }

                    items.Add(new Item(indexTarget, indexSource, null, null));
                    written.Add(writeable);
                }
            }

            // find the properties coming from the expressions of the select clause
            var count = written.Count;
            for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
                var columnName = selectExprForgeContext.ColumnNames[i];
                var forge = selectExprForgeContext.ExprForges[i];
                var exprNode = exprNodes[i];

                var writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find column '" + columnName + "' in target type '" + oaResultType.Name + "'");
                }

                TypeWidenerSPI widener;
                try {
                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
                        exprNode.Forge.EvaluationType,
                        writable.PropertyType,
                        columnName,
                        false,
                        null,
                        statementName);
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }

                items.Add(new Item(count, -1, forge, widener));
                written.Add(writable);
                count++;
            }

            // make manufacturer
            var itemsArr = items.ToArray();
            EventBeanManufacturerForge manufacturer;
            try {
                manufacturer = EventTypeUtility.GetManufacturer(
                    oaResultType,
                    written.ToArray(),
                    importService,
                    true,
                    selectExprForgeContext.EventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to write to type: " + e.Message, e);
            }

            return new OAInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
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