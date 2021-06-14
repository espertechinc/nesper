///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public interface ModuleAccessModifierService
    {
        NameAccessModifier GetAccessModifierEventType(
            StatementRawInfo raw,
            string eventTypeName);

        NameAccessModifier GetAccessModifierContext(
            StatementBaseInfo @base,
            string contextName);

        NameAccessModifier GetAccessModifierVariable(
            StatementBaseInfo @base,
            string variableName);

        NameAccessModifier GetAccessModifierExpression(
            StatementBaseInfo @base,
            string expressionName);

        NameAccessModifier GetAccessModifierTable(
            StatementBaseInfo @base,
            string tableName);

        NameAccessModifier GetAccessModifierNamedWindow(
            StatementBaseInfo @base,
            string namedWindowName);

        NameAccessModifier GetAccessModifierScript(
            StatementBaseInfo @base,
            string scriptName,
            int numParameters);

        EventTypeBusModifier GetBusModifierEventType(
            StatementRawInfo raw,
            string eventTypeName);
    }
} // end of namespace