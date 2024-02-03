///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.context.aifactory.createclass
{
    public class StatementAgentInstanceFactoryCreateClassForge
    {
        private readonly string className;

        private readonly EventType statementEventType;

        public StatementAgentInstanceFactoryCreateClassForge(
            EventType statementEventType,
            string className)
        {
            this.statementEventType = statementEventType;
            this.className = className;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    typeof(StatementAgentInstanceFactoryCreateClass),
                    GetType(),
                    "saiff",
                    parent,
                    symbols,
                    classScope)
                .Eventtype("statementEventType", statementEventType)
                .Constant("className", className)
                .BuildMethod();
        }
    }
} // end of namespace