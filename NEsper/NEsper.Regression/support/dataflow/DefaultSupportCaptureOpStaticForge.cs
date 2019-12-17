///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class DefaultSupportCaptureOpStaticForge<T> : DataFlowOperatorForge
    {
        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(DefaultSupportCaptureOpStaticFactory<T>));
        }
    }

    /// <summary>
    /// An implementation of the default support capture op static forge that is specific and
    /// generic for objects.  This **sadly** mirrors some of the mechanics of type erasure.
    /// </summary>
    public class DefaultSupportCaptureOpStaticForge : DefaultSupportCaptureOpStaticForge<object>
    {
    }
} // end of namespace