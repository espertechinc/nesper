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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Abstract specification on how to perform a table lookup.
    /// </summary>
    public abstract class TableLookupPlanForge : CodegenMakeable<SAIFFInitializeSymbol>
    {
        internal readonly int indexedStream;
        internal readonly int lookupStream;
        protected bool indexedStreamIsVDW;
        protected EventType[] typesPerStream;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupStream">stream number of stream that supplies event to be used to look up</param>
        /// <param name="indexedStream">stream number of stream that is being access via index/table</param>
        /// <param name="indexedStreamIsVDW">vdw indicators</param>
        /// <param name="typesPerStream">types</param>
        /// <param name="indexNum">index to use for lookup</param>
        protected TableLookupPlanForge(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableLookupIndexReqKey[] indexNum)
        {
            this.lookupStream = lookupStream;
            this.indexedStream = indexedStream;
            this.indexedStreamIsVDW = indexedStreamIsVDW;
            IndexNum = indexNum;
            this.typesPerStream = typesPerStream;
        }

        public abstract TableLookupKeyDesc KeyDescriptor { get; }

        /// <summary>
        ///     Returns the lookup stream.
        /// </summary>
        /// <returns>lookup stream</returns>
        public int LookupStream => lookupStream;

        /// <summary>
        ///     Returns indexed stream.
        /// </summary>
        /// <returns>indexed stream</returns>
        public int IndexedStream => indexedStream;

        /// <summary>
        ///     Returns index number to use for looking up in.
        /// </summary>
        /// <returns>index number</returns>
        public TableLookupIndexReqKey[] IndexNum { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfPlanFactory(), GetType(), classScope);
            IList<CodegenExpression> @params = new List<CodegenExpression>(6);
            @params.Add(Constant(lookupStream));
            @params.Add(Constant(indexedStream));
            @params.Add(
                CodegenMakeableUtil.MakeArray(
                    "reqIdxKeys",
                    typeof(TableLookupIndexReqKey),
                    IndexNum,
                    GetType(),
                    method,
                    symbols,
                    classScope));
            @params.AddAll(AdditionalParams(method, symbols, classScope));
            method.Block
                .DeclareVar(TypeOfPlanFactory(), "plan", NewInstance(TypeOfPlanFactory(), @params.ToArray()));

            // inject additional information for virtual data windows
            if (indexedStreamIsVDW) {
                var keyDesc = KeyDescriptor;
                var hashes = keyDesc.HashExpressions;
                var ranges = keyDesc.Ranges.ToArray();
                var rangeResults = QueryGraphValueEntryRangeForge.GetRangeResultTypes(ranges);
                method.Block
                    .SetProperty(
                        Ref("plan"),
                        "VirtualDWHashEvals",
                        ExprNodeUtilityCodegen.CodegenEvaluators(hashes, method, GetType(), classScope))
                    .SetProperty(
                        Ref("plan"),
                        "VirtualDWHashTypes",
                        Constant(ExprNodeUtilityQuery.GetExprResultTypes(hashes)))
                    .SetProperty(
                        Ref("plan"),
                        "VirtualDWRangeEvals",
                        QueryGraphValueEntryRangeForge.MakeArray(ranges, method, symbols, classScope))
                    .SetProperty(Ref("plan"), "VirtualDWRangeTypes", Constant(rangeResults));
            }

            method.Block.MethodReturn(Ref("plan"));
            return LocalMethod(method);
        }

        public abstract Type TypeOfPlanFactory();

        public abstract ICollection<CodegenExpression> AdditionalParams(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public override string ToString()
        {
            return "lookupStream=" +
                   lookupStream +
                   " indexedStream=" +
                   indexedStream +
                   " indexNum=" +
                   IndexNum.RenderAny();
        }
    }
} // end of namespace