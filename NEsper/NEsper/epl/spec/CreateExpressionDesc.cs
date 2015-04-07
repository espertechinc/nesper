///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class CreateExpressionDesc
    {
        public CreateExpressionDesc(ExpressionDeclItem expression)
        {
            Expression = expression;
            Script = null;
        }

        public CreateExpressionDesc(ExpressionScriptProvided script)
        {
            Script = script;
            Expression = null;
        }

        public CreateExpressionDesc(Pair<ExpressionDeclItem, ExpressionScriptProvided> pair)
        {
            Script = pair.Second;
            Expression = pair.First;
        }

        public ExpressionDeclItem Expression { get; private set; }

        public ExpressionScriptProvided Script { get; private set; }
    }
}