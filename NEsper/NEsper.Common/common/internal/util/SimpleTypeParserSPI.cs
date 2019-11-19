///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Parser of a String input to an object.
    /// </summary>
    public interface SimpleTypeParserSPI : SimpleTypeParser
    {
        CodegenExpression Codegen(CodegenExpression input);
    }

    public class ProxySimpleTypeParserSPI : ProxySimpleTypeParser,
        SimpleTypeParserSPI
    {
        public Func<CodegenExpression, CodegenExpression> ProcCodegen;
        public CodegenExpression Codegen(CodegenExpression input) => ProcCodegen?.Invoke(input);
    }
} // end of namespace