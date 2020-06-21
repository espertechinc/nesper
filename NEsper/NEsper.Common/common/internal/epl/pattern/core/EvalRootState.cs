///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    /// Interface for a root state node accepting a callback to use to indicate pattern results.
    /// </summary>
    public interface EvalRootState : StopCallback,
        EvalRootMatchRemover
    {
        /// <summary>
        /// Accept callback to indicate pattern results.
        /// </summary>
        /// <value>is a pattern result call</value>
        PatternMatchCallback Callback { set; }

        void StartRecoverable(
            bool startRecoverable,
            MatchedEventMap beginState);
    }

    public class EvalRootStateConstants
    {
        public static EvalRootState[] EMPTY = new EvalRootState[0];
    }
} // end of namespace