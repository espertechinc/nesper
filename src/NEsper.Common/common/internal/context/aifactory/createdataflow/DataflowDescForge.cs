///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createdataflow
{
    public class DataflowDescForge
    {
        private readonly string _dataflowName;
        private readonly IDictionary<string, EventType> _declaredTypes;
        private readonly IList<LogicalChannel> _logicalChannels;
        private readonly ISet<int> _operatorBuildOrder;
        private readonly IDictionary<int, OperatorMetadataDescriptor> _operatorMetadata;
        private readonly IDictionary<int, DataFlowOperatorForge> _operatorFactories;
        private readonly IList<StmtForgeMethodResult> _forgables;
        private readonly IList<StmtClassForgeableFactory> _additionalForgables;

        public DataflowDescForge(
            string dataflowName,
            IDictionary<string, EventType> declaredTypes,
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
            ISet<int> operatorBuildOrder,
            IDictionary<int, DataFlowOperatorForge> operatorFactories,
            IList<LogicalChannel> logicalChannels,
            IList<StmtForgeMethodResult> forgables,
            IList<StmtClassForgeableFactory> additionalForgables)
        {
            _dataflowName = dataflowName;
            _declaredTypes = declaredTypes;
            _operatorMetadata = operatorMetadata;
            _operatorBuildOrder = operatorBuildOrder;
            _operatorFactories = operatorFactories;
            _logicalChannels = logicalChannels;
            _forgables = forgables;
            _additionalForgables = additionalForgables;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(DataflowDesc), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(typeof(DataflowDesc), "df")
                .SetProperty(Ref("df"), "DataflowName", Constant(_dataflowName))
                .SetProperty(Ref("df"), "DeclaredTypes", MakeTypes(_declaredTypes, method, symbols, classScope))
                .SetProperty(
                    Ref("df"),
                    "OperatorMetadata",
                    MakeOpMeta(_operatorMetadata, method, symbols, classScope))
                .SetProperty(
                    Ref("df"),
                    "OperatorBuildOrder",
                    MakeOpBuildOrder(_operatorBuildOrder, method, symbols, classScope))
                .SetProperty(
                    Ref("df"),
                    "OperatorFactories",
                    MakeOpFactories(_operatorFactories, method, symbols, classScope))
                .SetProperty(
                    Ref("df"),
                    "LogicalChannels",
                    MakeOpChannels(_logicalChannels, method, symbols, classScope))
                .MethodReturn(Ref("df"));
            return LocalMethod(method);
        }

        public IDictionary<int, DataFlowOperatorForge> OperatorFactories => _operatorFactories;

        public IList<StmtForgeMethodResult> Forgables => _forgables;

        public IList<StmtClassForgeableFactory> AdditionalForgables => _additionalForgables;

        private static CodegenExpression MakeOpChannels(
            IList<LogicalChannel> logicalChannels,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(IList<LogicalChannel>), typeof(DataflowDescForge), classScope);
            method.Block.DeclareVar<IList<LogicalChannel>>(
                "chnl",
                NewInstance(typeof(List<LogicalChannel>), Constant(logicalChannels.Count)));
            foreach (var channel in logicalChannels) {
                method.Block.ExprDotMethod(Ref("chnl"), "Add", channel.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("chnl"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeOpBuildOrder(
            ISet<int> operatorBuildOrder,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(LinkedHashSet<int>), typeof(DataflowDescForge), classScope);
            method.Block.DeclareVar<LinkedHashSet<int>>(
                "order",
                NewInstance(
                    typeof(LinkedHashSet<int>)));
            foreach (var entry in operatorBuildOrder) {
                method.Block.ExprDotMethod(Ref("order"), "Add", Constant(entry));
            }

            method.Block.MethodReturn(Ref("order"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeOpFactories(
            IDictionary<int, DataFlowOperatorForge> operatorFactories,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(IDictionary<int, DataFlowOperatorFactory>), typeof(DataflowDescForge), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<int, DataFlowOperatorFactory>),
                "fac",
                NewInstance(
                    typeof(Dictionary<int, DataFlowOperatorFactory>)));
            foreach (var entry in operatorFactories) {
                method.Block.ExprDotMethod(
                    Ref("fac"),
                    "Put",
                    Constant(entry.Key),
                    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("fac"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeOpMeta(
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(IDictionary<int, OperatorMetadataDescriptor>), typeof(DataflowDescForge), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<int, OperatorMetadataDescriptor>),
                "op",
                NewInstance(
                    typeof(Dictionary<int, OperatorMetadataDescriptor>)));
            foreach (var entry in operatorMetadata) {
                method.Block.ExprDotMethod(
                    Ref("op"),
                    "Put",
                    Constant(entry.Key),
                    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("op"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeTypes(
            IDictionary<string, EventType> declaredTypes,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IDictionary<string, EventType>), typeof(DataflowDescForge), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<string, EventType>),
                "types",
                NewInstance(
                    typeof(Dictionary<string, EventType>)));
            foreach (var entry in declaredTypes) {
                method.Block.ExprDotMethod(
                    Ref("types"),
                    "Put",
                    Constant(entry.Key),
                    EventTypeUtility.ResolveTypeCodegen(entry.Value, symbols.GetAddInitSvc(method)));
            }

            method.Block.MethodReturn(Ref("types"));
            return LocalMethod(method);
        }
    }
} // end of namespace