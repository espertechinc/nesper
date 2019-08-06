///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public interface FAFQueryMethodAssignerSetter
    {
        void Assign(StatementAIFactoryAssignments assignments);

        void SetValue(
            int number,
            object value);
    }

    public class ProxyFAFQueryMethodAssignerSetter : FAFQueryMethodAssignerSetter
    {
        public delegate void AssignFunc(StatementAIFactoryAssignments assignments);
        public delegate void SetValueFunc(int number, object value);

        public AssignFunc ProcAssign { get; set; }
        public SetValueFunc ProcSetValue { get; set; }

        public ProxyFAFQueryMethodAssignerSetter()
        {
        }

        public ProxyFAFQueryMethodAssignerSetter(
            AssignFunc procAssign,
            SetValueFunc procSetValue)
        {
            ProcAssign = procAssign;
            ProcSetValue = procSetValue;
        }

        public void Assign(StatementAIFactoryAssignments assignments)
        {
            ProcAssign(assignments);
        }

        public void SetValue(
            int number,
            object value)
        {
            ProcSetValue(number, value);
        }
    }
} // end of namespace