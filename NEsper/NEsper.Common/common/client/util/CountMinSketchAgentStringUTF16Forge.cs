///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Count-min sketch agent that handles String-type values and uses UTF-16 encoding
    /// to transform strings to byte-array and back.
    /// </summary>
    public class CountMinSketchAgentStringUTF16Forge : CountMinSketchAgentForge
    {
        public Type[] AcceptableValueTypes {
            get { return new Type[] {typeof(string)}; }
        }

        public CodegenExpression CodegenMake(
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(CountMinSketchAgentStringUTF16));
        }
    }
} // end of namespace