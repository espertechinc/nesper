///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    public class EPStatementFactoryDefault : EPStatementFactory
    {
        public static readonly EPStatementFactoryDefault INSTANCE = new EPStatementFactoryDefault();

        private EPStatementFactoryDefault()
        {
        }

        public EPStatementSPI Statement(EPStatementFactoryArgs args)
        {
            return new EPStatementImpl(args);
        }
    }
} // end of namespace