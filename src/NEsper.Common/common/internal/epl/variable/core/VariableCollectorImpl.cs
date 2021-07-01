///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableCollectorImpl : VariableCollector
    {
        private readonly IDictionary<string, VariableMetaData> moduleVariables;

        public VariableCollectorImpl(IDictionary<string, VariableMetaData> moduleVariables)
        {
            this.moduleVariables = moduleVariables;
        }

        public void RegisterVariable(
            string variableName,
            VariableMetaData variableMetaData)
        {
            moduleVariables.Put(variableName, variableMetaData);
        }
    }
} // end of namespace