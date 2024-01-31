///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableMetadataWithForgables
    {
        private readonly VariableMetaData variableMetaData;
        private readonly IList<StmtClassForgeableFactory> forgables;

        public VariableMetadataWithForgables(
            VariableMetaData variableMetaData,
            IList<StmtClassForgeableFactory> forgables)
        {
            this.variableMetaData = variableMetaData;
            this.forgables = forgables;
        }

        public VariableMetaData VariableMetaData => variableMetaData;

        public IList<StmtClassForgeableFactory> Forgables => forgables;
    }
} // end of namespace