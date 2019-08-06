///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    /// <summary>
    ///     Analysis result for joins.
    /// </summary>
    public class StreamJoinAnalysisResultCompileTime
    {
        private readonly bool[] unidirectionalNonDriving;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        public StreamJoinAnalysisResultCompileTime(int numStreams)
        {
            NumStreams = numStreams;
            IsPureSelfJoin = false;
            UnidirectionalInd = new bool[numStreams];
            unidirectionalNonDriving = new bool[numStreams];
            HasChildViews = new bool[numStreams];
            NamedWindowsPerStream = new NamedWindowMetaData[numStreams];
            UniqueKeys = new string[numStreams][][];
            TablesPerStream = new TableMetaData[numStreams];
        }

        /// <summary>
        ///     Returns unidirection ind.
        /// </summary>
        /// <returns>unidirectional flags</returns>
        public bool[] UnidirectionalInd { get; }

        /// <summary>
        ///     Returns child view flags.
        /// </summary>
        /// <returns>flags</returns>
        public bool[] HasChildViews { get; }

        public NamedWindowMetaData[] NamedWindowsPerStream { get; }

        /// <summary>
        ///     Returns streams num.
        /// </summary>
        /// <returns>num</returns>
        public int NumStreams { get; }

        public string[][][] UniqueKeys { get; }

        public TableMetaData[] TablesPerStream { get; }

        public bool IsUnidirectional {
            get {
                foreach (var ind in UnidirectionalInd) {
                    if (ind) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsUnidirectionalAll {
            get { return UnidirectionalInd.All(ind => ind); }
        }

        /// <summary>
        ///     Sets self-join.
        /// </summary>
        /// <value>if a self join</value>
        public bool IsPureSelfJoin { get; set; }

        public int UnidirectionalCount {
            get {
                var count = 0;
                foreach (var ind in UnidirectionalInd) {
                    count += ind ? 1 : 0;
                }

                return count;
            }
        }

        /// <summary>
        ///     Sets flag.
        /// </summary>
        /// <param name="index">index</param>
        public void SetUnidirectionalInd(int index)
        {
            UnidirectionalInd[index] = true;
        }

        /// <summary>
        ///     Sets flag.
        /// </summary>
        /// <param name="index">index</param>
        public void SetUnidirectionalNonDriving(int index)
        {
            unidirectionalNonDriving[index] = true;
        }

        /// <summary>
        ///     Sets child view flags.
        /// </summary>
        /// <param name="index">to set</param>
        public void SetHasChildViews(int index)
        {
            HasChildViews[index] = true;
        }

        public void SetNamedWindowsPerStream(
            int streamNum,
            NamedWindowMetaData metadata)
        {
            NamedWindowsPerStream[streamNum] = metadata;
        }

        public void SetTablesForStream(
            int streamNum,
            TableMetaData metadata)
        {
            TablesPerStream[streamNum] = metadata;
        }

        public void AddUniquenessInfo(
            IList<ViewFactoryForge>[] unmaterializedViewChain,
            Attribute[] annotations)
        {
            for (var i = 0; i < unmaterializedViewChain.Length; i++) {
                var uniquenessProps = GetUniqueCandidateProperties(unmaterializedViewChain[i], annotations);
                if (uniquenessProps != null) {
                    UniqueKeys[i] = new string[1][];
                    UniqueKeys[i][0] = uniquenessProps.ToArray();
                }
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StreamJoinAnalysisResultRuntime), GetType(), classScope);
            method.Block
                .DeclareVar<StreamJoinAnalysisResultRuntime>(
                    "ar",
                    NewInstance(typeof(StreamJoinAnalysisResultRuntime)))
                .SetProperty(Ref("ar"), "IsPureSelfJoin", Constant(IsPureSelfJoin))
                .SetProperty(Ref("ar"), "Unidirectional", Constant(UnidirectionalInd))
                .SetProperty(Ref("ar"), "UnidirectionalNonDriving", Constant(unidirectionalNonDriving))
                .SetProperty(Ref("ar"), "NamedWindows", MakeNamedWindows(method, symbols))
                .SetProperty(Ref("ar"), "Tables", MakeTables(method, symbols))
                .MethodReturn(Ref("ar"));
            return LocalMethod(method);
        }

        public bool IsVirtualDW(int stream)
        {
            return NamedWindowsPerStream[stream] != null && NamedWindowsPerStream[stream].IsVirtualDataWindow;
        }

        private CodegenExpression MakeTables(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols)
        {
            var init = new CodegenExpression[TablesPerStream.Length];
            for (var i = 0; i < init.Length; i++) {
                init[i] = TablesPerStream[i] == null
                    ? ConstantNull()
                    : TableDeployTimeResolver.MakeResolveTable(TablesPerStream[i], symbols.GetAddInitSvc(method));
            }

            return NewArrayWithInit(typeof(Table), init);
        }

        private CodegenExpression MakeNamedWindows(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols)
        {
            var init = new CodegenExpression[NamedWindowsPerStream.Length];
            for (var i = 0; i < init.Length; i++) {
                init[i] = NamedWindowsPerStream[i] == null
                    ? ConstantNull()
                    : NamedWindowDeployTimeResolver.MakeResolveNamedWindow(
                        NamedWindowsPerStream[i],
                        symbols.GetAddInitSvc(method));
            }

            return NewArrayWithInit(typeof(NamedWindow), init);
        }

        public static ISet<string> GetUniqueCandidateProperties(
            IList<ViewFactoryForge> forges,
            Attribute[] annotations)
        {
            var disableUniqueImplicit = HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX.GetHint(annotations) != null;
            if (forges == null || forges.IsEmpty()) {
                return null;
            }

            if (forges[0] is GroupByViewFactoryForge) {
                var grouped = (GroupByViewFactoryForge) forges[0];
                var criteria = grouped.CriteriaExpressions;
                var groupedCriteria = ExprNodeUtilityQuery.GetPropertyNamesIfAllProps(criteria);
                if (groupedCriteria == null) {
                    return null;
                }

                var inner = grouped.Groupeds[0];
                if (inner is DataWindowViewForgeUniqueCandidate && !disableUniqueImplicit) {
                    var uniqueFactory = (DataWindowViewForgeUniqueCandidate) inner;
                    var uniqueCandidates = uniqueFactory.UniquenessCandidatePropertyNames;
                    if (uniqueCandidates != null) {
                        uniqueCandidates.AddAll(groupedCriteria);
                    }

                    return uniqueCandidates;
                }

                return null;
            }

            if (forges[0] is DataWindowViewForgeUniqueCandidate && !disableUniqueImplicit) {
                var uniqueFactory = (DataWindowViewForgeUniqueCandidate) forges[0];
                return uniqueFactory.UniquenessCandidatePropertyNames;
            }

            if (forges[0] is VirtualDWViewFactoryForge) {
                var vdw = (VirtualDWViewFactoryForge) forges[0];
                return vdw.UniqueKeys;
            }

            return null;
        }
    }
} // end of namespace