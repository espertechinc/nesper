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
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.firstlength
{
    /// <summary>
    ///     Factory for <seealso cref="FirstLengthWindowView" />.
    /// </summary>
    public class FirstLengthWindowViewForge : ViewFactoryForgeBase,
        AsymetricDataWindowViewForge
    {
        internal ExprForge sizeForge;

        public override string ViewName => "First-Length";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            sizeForge = ViewForgeSupport.ValidateSizeSingleParam(ViewName, parameters, viewForgeEnv, streamNumber);
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(FirstLengthWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "firstlength";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var sizeEval = CodegenEvaluator(sizeForge, method, GetType(), classScope);
            method.Block.SetProperty(factory, "Size", sizeEval);
        }
    }
} // end of namespace