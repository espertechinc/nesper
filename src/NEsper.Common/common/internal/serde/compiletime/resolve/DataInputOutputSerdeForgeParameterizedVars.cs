///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeParameterizedVars
    {
        public DataInputOutputSerdeForgeParameterizedVars(
            CodegenMethod method,
            CodegenClassScope scope,
            CodegenExpression optionalEventTypeResolver)
        {
            Method = method;
            Scope = scope;
            OptionalEventTypeResolver = optionalEventTypeResolver;
        }

        public CodegenMethod Method { get; }

        public CodegenClassScope Scope { get; }

        public CodegenExpression OptionalEventTypeResolver { get; }
    }
} // end of namespace