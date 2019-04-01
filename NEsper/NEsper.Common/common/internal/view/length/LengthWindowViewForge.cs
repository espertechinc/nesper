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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.length
{
    public class LengthWindowViewForge : ViewFactoryForgeBase, DataWindowViewForge, DataWindowViewForgeWithPrevious
    {
        private ExprForge sizeForge;

        public override void SetViewParameters(IList<ExprNode> parameters, ViewForgeEnv viewForgeEnv, int streamNumber)
        {
            sizeForge = ViewForgeSupport.ValidateSizeSingleParam(ViewName, parameters, viewForgeEnv, streamNumber);
        }

        public override void Attach(EventType parentEventType, int streamNumber, ViewForgeEnv viewForgeEnv)
        {
            this.eventType = parentEventType;
        }

        public override string ViewName
        {
            get => "Length";
        }

        internal override Type TypeOfFactory()
        {
            return typeof(LengthWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "length";
        }

        internal override void Assign(CodegenMethod method, CodegenExpressionRef factory, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            CodegenExpressionNewAnonymousClass sizeEval = ExprNodeUtilityCodegen
                .CodegenEvaluator(sizeForge, method, this.GetType(), classScope);
            method.Block.ExprDotMethod(factory, "setSize", sizeEval);
        }
    }
} // end of namespace