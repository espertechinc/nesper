///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.json.write
{
    public interface JsonWriteForge
    {
        CodegenExpression CodegenWrite(
            JsonWriteForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope);
    }

    public class ProxyJsonWriteForge : JsonWriteForge
    {
        public Func<JsonWriteForgeRefs, CodegenMethod, CodegenClassScope, CodegenExpression> ProcCodegenWrite { get; set; }

        public ProxyJsonWriteForge()
        {
        }

        public ProxyJsonWriteForge(Func<JsonWriteForgeRefs, CodegenMethod, CodegenClassScope, CodegenExpression> procCodegenWrite)
        {
            ProcCodegenWrite = procCodegenWrite;
        }

        public CodegenExpression CodegenWrite(
            JsonWriteForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope) => ProcCodegenWrite(refs, method, classScope);

    }
} // end of namespace