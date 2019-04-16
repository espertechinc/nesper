///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class StatementSpecCompileException : Exception
    {
        public StatementSpecCompileException(
            string message,
            string expression)
            : base(message)
        {
            Expression = expression;
        }

        public StatementSpecCompileException(
            string message,
            Exception cause,
            string expression)
            : base(message, cause)
        {
            Expression = expression;
        }

        public string Expression { get; }
    }
} // end of namespace