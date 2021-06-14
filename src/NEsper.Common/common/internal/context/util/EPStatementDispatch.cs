///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Interface for statement-level dispatch.
    /// <para/>
    /// Relevant when a statements callbacks have completed and the join processing must take place.
    /// </summary>
    public interface EPStatementDispatch
    {
        /// <summary>
        /// Execute dispatch.
        /// </summary>
        void Execute();
    }

    public class ProxyEPStatementDispatch : EPStatementDispatch
    {
        public Action ProcExecute { get; set; }

        /// <summary>
        /// Execute dispatch.
        /// </summary>
        public void Execute()
        {
            ProcExecute();
        }
    }
}