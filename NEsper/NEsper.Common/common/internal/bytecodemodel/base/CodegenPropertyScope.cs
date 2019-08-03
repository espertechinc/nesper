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
        CodegenProperty MakeChild(
            string returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenProperty MakeChild(
            Type returnType,
            Type generator,
            CodegenScope codegenClassScope);

        CodegenProperty MakeChildWithScope(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope codegenClassScope);

        CodegenProperty AddSymbol(CodegenExpressionRef symbol);
    }
} // end of namespace