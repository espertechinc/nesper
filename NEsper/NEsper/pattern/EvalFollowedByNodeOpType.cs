///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>Followed-by operator types in the evaluation tree representing any event expressions. </summary>
    public enum EvalFollowedByNodeOpType {
        NOMAX_PLAIN,
        MAX_PLAIN,
        NOMAX_POOL,
        MAX_POOL
    }
}
