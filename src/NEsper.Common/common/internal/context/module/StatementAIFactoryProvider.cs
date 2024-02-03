///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface StatementAIFactoryProvider
    {
        StatementAgentInstanceFactory Factory { get; }

        void Assign(StatementAIFactoryAssignments assignments)
        {
        }

        void Unassign()
        {
        }

        void SetValue(
            int number,
            object value)
        {
        }
    }
} // end of namespace