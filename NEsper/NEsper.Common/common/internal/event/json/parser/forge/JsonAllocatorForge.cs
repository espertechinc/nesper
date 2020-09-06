///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.forge;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public interface JsonAllocatorForge
    {
        CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod method,
            CodegenClassScope classScope);
    }
} // end of namespace