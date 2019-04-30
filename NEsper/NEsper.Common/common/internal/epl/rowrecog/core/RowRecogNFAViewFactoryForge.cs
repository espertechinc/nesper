///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     View factory for match-recognize view.
    /// </summary>
    public class RowRecogNFAViewFactoryForge : ViewFactoryForgeBase,
        ScheduleHandleCallbackProvider
    {
        private readonly RowRecogDescForge rowRecogDescForge;
        private int scheduleCallbackId = -1;

        public RowRecogNFAViewFactoryForge(RowRecogDescForge rowRecogDescForge)
        {
            this.rowRecogDescForge = rowRecogDescForge;
            eventType = rowRecogDescForge.RowEventType;
        }

        public override string ViewName => "match-recognize";

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            // no action
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            // no action
        }

        internal override Type TypeOfFactory()
        {
            return typeof(RowRecogNFAViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "rowRecog";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (scheduleCallbackId == -1) {
                throw new IllegalStateException("No schedule callback id");
            }

            method.Block
                .SetProperty(factory, "Desc", rowRecogDescForge.Make(method, symbols, classScope))
                .SetProperty(factory, "ScheduleCallbackId", Constant(scheduleCallbackId));
        }

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace