///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public interface CodegenMethodScope
    {
        CodegenMethod MakeChild(
            string returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenMethod MakeChild(
            Type returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenMethod MakeChildWithScope(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope codegenClassScope);

        CodegenMethodScope AddSymbol(
            CodegenExpressionRef symbol);
    }
} // end of namespace