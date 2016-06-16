///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Method to call to destroy an EPStatement.
    /// </summary>
    public interface EPStatementDestroyMethod
    {
        /// <summary>
        /// Destroys a statement
        /// </summary>
        void Destroy();
    }

    public sealed class ProxyEPStatementDestroyMethod : EPStatementDestroyMethod
    {
        public Action ProcDestroy;

        public ProxyEPStatementDestroyMethod() { }
        public ProxyEPStatementDestroyMethod(Action procDestroy)
        {
            ProcDestroy = procDestroy;
        }

        public void Destroy()
        {
            ProcDestroy.Invoke();
        }
    }
}
