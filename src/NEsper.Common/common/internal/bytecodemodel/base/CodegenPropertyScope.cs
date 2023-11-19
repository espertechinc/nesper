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
    public interface CodegenPropertyScope
    {
        CodegenMethod MakeChildMethod(
            string returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenMethod MakeChildMethod(
            Type returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenMethod MakeChildMethodWithScope(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope codegenClassScope);

        CodegenProperty AddSymbol(CodegenExpressionRef symbol);
    }
} // end of namespace