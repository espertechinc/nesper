///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.declexpr
{
    public interface ExprDeclaredService
    {
        ExpressionDeclItem GetExpression(String name);
        IList<ExpressionScriptProvided> GetScriptsByName(String expressionName);
        String AddExpressionOrScript(CreateExpressionDesc expression);
        void DestroyedExpression(CreateExpressionDesc expression);
        void Dispose();
    }
}