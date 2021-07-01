///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    public class VariableCompileTimeRegistry : CompileTimeRegistry
    {
        public IDictionary<string, VariableMetaData> Variables { get; } = new HashMap<string, VariableMetaData>();

        public void NewVariable(VariableMetaData metaData)
        {
            if (!metaData.VariableVisibility.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for variables");
            }

            var existing = Variables.Get(metaData.VariableName);
            if (existing != null) {
                throw new IllegalStateException(
                    "Duplicate variable definition for name '" + metaData.VariableName + "'");
            }

            Variables.Put(metaData.VariableName, metaData);
        }

        public VariableMetaData GetVariable(string variableName)
        {
            return Variables.Get(variableName);
        }
    }
} // end of namespace