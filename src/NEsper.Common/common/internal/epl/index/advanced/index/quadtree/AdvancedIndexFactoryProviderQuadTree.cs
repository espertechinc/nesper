///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using static com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree.AdvancedIndexQuadTreeConstants;
using static com.espertech.esper.common.@internal.epl.index.advanced.index.service.AdvancedIndexEvaluationHelper;
using static com.espertech.esper.common.@internal.epl.index.advanced.index.service.AdvancedIndexValidationHelper;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public abstract class AdvancedIndexFactoryProviderQuadTree : AdvancedIndexFactoryProvider
    {
        public AdvancedIndexConfigContextPartition ValidateConfigureFilterIndex(
            string indexName,
            string indexTypeName,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            ValidateParameters(indexTypeName, parameters);
            try {
                return ConfigureQuadTree(indexName, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parameters), null);
            }
            catch (EPException ex) {
                throw new ExprValidationException(ex.Message, ex);
            }
        }

        public abstract EventAdvancedIndexProvisionCompileTime ValidateEventIndex(
            string indexName,
            string indexTypeName,
            ExprNode[] columns,
            ExprNode[] parameters);

        protected internal static void ValidateParameters(
            string indexTypeName,
            ExprNode[] parameters)
        {
            ValidateParameterCount(4, 6, indexTypeName, parameters == null ? 0 : parameters.Length);
            ValidateParameterReturnTypeNumber(indexTypeName, 0, parameters[0], PARAM_XMIN);
            ValidateParameterReturnTypeNumber(
                indexTypeName,
                1,
                parameters[1],
                PARAM_YMIN);
            ValidateParameterReturnTypeNumber(indexTypeName, 2, parameters[2], PARAM_WIDTH);
            ValidateParameterReturnTypeNumber(
                indexTypeName,
                3,
                parameters[3],
                PARAM_HEIGHT);
            if (parameters.Length > 4) {
                ValidateParameterReturnType(typeof(int?), indexTypeName, 4, parameters[4], PARAM_LEAFCAPACITY);
            }

            if (parameters.Length > 5) {
                ValidateParameterReturnType(typeof(int?), indexTypeName, 5, parameters[5], PARAM_MAXTREEHEIGHT);
            }
        }

        protected internal static AdvancedIndexConfigContextPartition ConfigureQuadTree(
            string indexName,
            ExprEvaluator[] parameters,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            double x = EvalDoubleParameter(parameters[0], indexName, PARAM_XMIN, exprEvaluatorContext);
            double y = EvalDoubleParameter(parameters[1], indexName, PARAM_YMIN, exprEvaluatorContext);
            double width = EvalDoubleParameter(parameters[2], indexName, PARAM_WIDTH, exprEvaluatorContext);
            if (width <= 0) {
                throw InvalidParameterValue(indexName, PARAM_WIDTH, width, "value>0");
            }

            double height = EvalDoubleParameter(parameters[3], indexName, PARAM_HEIGHT, exprEvaluatorContext);
            if (height <= 0) {
                throw InvalidParameterValue(indexName, PARAM_HEIGHT, height, "value>0");
            }

            int leafCapacity = parameters.Length > 4
                ? EvalIntParameter(parameters[4], indexName, PARAM_LEAFCAPACITY, exprEvaluatorContext)
                : PointRegionQuadTreeFactory<object>.DEFAULT_LEAF_CAPACITY;
            if (leafCapacity < 1) {
                throw InvalidParameterValue(indexName, PARAM_LEAFCAPACITY, leafCapacity, "value>=1");
            }

            int maxTreeHeight = parameters.Length > 5
                ? EvalIntParameter(parameters[5], indexName, PARAM_MAXTREEHEIGHT, exprEvaluatorContext)
                : PointRegionQuadTreeFactory<object>.DEFAULT_MAX_TREE_HEIGHT;
            if (maxTreeHeight < 2) {
                throw InvalidParameterValue(indexName, PARAM_MAXTREEHEIGHT, maxTreeHeight, "value>=2");
            }

            return new AdvancedIndexConfigContextPartitionQuadTree(x, y, width, height, leafCapacity, maxTreeHeight);
        }
    }
} // end of namespace