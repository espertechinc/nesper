///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByLevelForge
    {
        public AggregationLocalGroupByLevelForge(
            ExprForge[][] methodForges,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessStateForges,
            ExprNode[] partitionForges,
            MultiKeyClassRef partitionMkClasses,
            bool defaultLevel)
        {
            MethodForges = methodForges;
            MethodFactories = methodFactories;
            AccessStateForges = accessStateForges;
            PartitionForges = partitionForges;
            PartitionMKClasses = partitionMkClasses;
            IsDefaultLevel = defaultLevel;
        }

        public ExprForge[][] MethodForges { get; }

        public AggregationForgeFactory[] MethodFactories { get; }

        public AggregationStateFactoryForge[] AccessStateForges { get; }

        public ExprNode[] PartitionForges { get; }
        
        public MultiKeyClassRef PartitionMKClasses { get; }

        public bool IsDefaultLevel { get; }

        public CodegenExpression ToExpression(
            string rowFactory,
            string rowSerde,
            CodegenExpression groupKeyEval,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return NewInstance<AggregationLocalGroupByLevel>(
                NewInstanceNamed(rowFactory, Ref("this")),
                NewInstanceNamed(rowSerde, Ref("this")),
                Constant(ExprNodeUtilityQuery.GetExprResultTypes(PartitionForges)),
                groupKeyEval,
                Constant(IsDefaultLevel),
                PartitionMKClasses.GetExprMKSerde(method, classScope));
        }
    }
} // end of namespace