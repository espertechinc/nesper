///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    [Serializable]
    public class StatementSpecCompileSyntaxException : StatementSpecCompileException
    {
        public StatementSpecCompileSyntaxException(
            string message,
            string expression)
            : base(message, expression)
        {
        }

        public StatementSpecCompileSyntaxException(
            string message,
            Exception cause,
            string expression)
            : base(message, cause, expression)
        {
        }

        protected StatementSpecCompileSyntaxException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace