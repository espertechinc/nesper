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

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
    public interface JsonSerializerForge
    {
        CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope);
    }

    public class ProxyJsonSerializerForge : JsonSerializerForge
    {
        public Func<JsonSerializerForgeRefs, CodegenMethod, CodegenClassScope, CodegenExpression> ProcCodegenWrite { get; set; }

        public ProxyJsonSerializerForge()
        {
        }

        public ProxyJsonSerializerForge(Func<JsonSerializerForgeRefs, CodegenMethod, CodegenClassScope, CodegenExpression> procCodegenWrite)
        {
            ProcCodegenWrite = procCodegenWrite;
        }

        public CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope) => ProcCodegenWrite(refs, method, classScope);
    }
} // end of namespace