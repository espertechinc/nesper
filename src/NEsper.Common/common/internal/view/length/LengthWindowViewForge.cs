///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.length
{
    public class LengthWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        private ExprForge sizeForge;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            sizeForge = ViewForgeSupport.ValidateSizeSingleParam(ViewName, parameters, viewForgeEnv, streamNumber);
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        public override string ViewName => "Length";

        internal override Type TypeOfFactory => typeof(LengthWindowViewFactory);

        internal override string FactoryMethod => "Length";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var sizeEval = ExprNodeUtilityCodegen
                .CodegenEvaluator(sizeForge, method, GetType(), classScope);
            method.Block.SetProperty(factory, "SizeEvaluator", sizeEval);
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_LENGTH;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace