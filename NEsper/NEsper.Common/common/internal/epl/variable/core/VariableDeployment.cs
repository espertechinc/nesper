///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableDeployment
    {
        public IDictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>(4);

        public void AddVariable(string variableName, Variable variable)
        {
            Variables.Put(variableName, variable);
        }

        public Variable GetVariable(string variableName)
        {
            return Variables.Get(variableName);
        }

        public void Remove(string variableName)
        {
            Variables.Remove(variableName);
        }
    }
} // end of namespace