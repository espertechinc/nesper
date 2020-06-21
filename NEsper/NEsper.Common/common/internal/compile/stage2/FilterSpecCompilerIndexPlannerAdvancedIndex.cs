///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper; //getIdentNodeDoubleEval

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerAdvancedIndex
    {
        protected static FilterSpecParamForge HandleAdvancedIndexDescProvider(
            FilterSpecCompilerAdvIndexDescProvider provider,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            var filterDesc = provider.FilterSpecDesc;
            if (filterDesc == null) {
                return null;
            }

            var keyExpressions = filterDesc.KeyExpressions;
            var xGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[0]);
            var yGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[1]);
            var widthGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[2]);
            var heightGetter = ResolveFilterIndexRequiredGetter(filterDesc.IndexName, keyExpressions[3]);
            var config = (AdvancedIndexConfigContextPartitionQuadTree) filterDesc.IndexSpec;

            var builder = new StringBuilder();
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[0], builder);
            builder.Append(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[1], builder);
            builder.Append(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[2], builder);
            builder.Append(",");
            ExprNodeUtilityPrint.ToExpressionString(keyExpressions[3], builder);
            builder.Append("/");
            builder.Append(filterDesc.IndexName.ToLowerInvariant());
            builder.Append("/");
            builder.Append(filterDesc.IndexType.ToLowerInvariant());
            builder.Append("/");
            config.ToConfiguration(builder);
            var expression = builder.ToString();

            Type returnType;
            switch (filterDesc.IndexType) {
                case SettingsApplicationDotMethodPointInsideRectange.INDEXTYPE_NAME:
                    returnType = typeof(XYPoint);
                    break;

                case SettingsApplicationDotMethodRectangeIntersectsRectangle.INDEXTYPE_NAME:
                    returnType = typeof(XYWHRectangle);
                    break;

                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }

            var lookupable = new FilterSpecLookupableAdvancedIndexForge(
                expression,
                null,
                returnType,
                config,
                xGetter,
                yGetter,
                widthGetter,
                heightGetter,
                filterDesc.IndexType);

            var indexExpressions = filterDesc.IndexExpressions;
            var xEval = ResolveFilterIndexDoubleEval(
                filterDesc.IndexName,
                indexExpressions[0],
                arrayEventTypes,
                statementName);
            var yEval = ResolveFilterIndexDoubleEval(
                filterDesc.IndexName,
                indexExpressions[1],
                arrayEventTypes,
                statementName);
            switch (filterDesc.IndexType) {
                case SettingsApplicationDotMethodPointInsideRectangle.INDEXTYPE_NAME:
                    return new FilterSpecParamAdvancedIndexQuadTreePointRegionForge(lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval);

                case SettingsApplicationDotMethodRectangeIntersectsRectangle.INDEXTYPE_NAME:
                    var widthEval = ResolveFilterIndexDoubleEval(
                        filterDesc.IndexName,
                        indexExpressions[2],
                        arrayEventTypes,
                        statementName);
                    var heightEval = ResolveFilterIndexDoubleEval(
                        filterDesc.IndexName,
                        indexExpressions[3],
                        arrayEventTypes,
                        statementName);
                    return new FilterSpecParamAdvancedIndexQuadTreeMXCIFForge(lookupable, FilterOperator.ADVANCED_INDEX, xEval, yEval, widthEval, heightEval);

                default:
                    throw new IllegalStateException("Unrecognized index type " + filterDesc.IndexType);
            }
        }

        private static FilterSpecParamFilterForEvalDoubleForge ResolveFilterIndexDoubleEval(
            string indexName,
            ExprNode indexExpression,
            LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes,
            string statementName)
        {
            FilterSpecParamFilterForEvalDoubleForge resolved = null;
            if (indexExpression is ExprIdentNode) {
                resolved = GetIdentNodeDoubleEval((ExprIdentNode) indexExpression, arrayEventTypes, statementName);
            }
            else if (indexExpression is ExprContextPropertyNode) {
                var node = (ExprContextPropertyNode) indexExpression;
                resolved = new FilterForEvalContextPropDoubleForge(node.Getter, node.PropertyName);
            }
            else if (indexExpression.Forge.ForgeConstantType.IsCompileTimeConstant) {
                double d = ((Number) indexExpression.Forge.ExprEvaluator.Evaluate(null, true, null)).DoubleValue();
                resolved = new FilterForEvalConstantDoubleForge(d);
            }
            else if (indexExpression.Forge.ForgeConstantType.IsConstant) {
                resolved = new FilterForEvalConstRuntimeExprForge(indexExpression);
            }

            if (resolved != null) {
                return resolved;
            }

            throw new ExprValidationException(
                "Invalid filter-indexable expression '" +
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(indexExpression) +
                "' in respect to index '" +
                indexName +
                "': expected either a constant, context-builtin or property from a previous pattern match");
        }

        private static EventPropertyGetterSPI ResolveFilterIndexRequiredGetter(
            string indexName,
            ExprNode keyExpression)
        {
            if (!(keyExpression is ExprIdentNode)) {
                throw new ExprValidationException(
                    "Invalid filter-index lookup expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(keyExpression) +
                    "' in respect to index '" +
                    indexName +
                    "': expected an event property name");
            }

            return ((ExprIdentNode) keyExpression).ExprEvaluatorIdent.Getter;
        }
    }
} // end of namespace