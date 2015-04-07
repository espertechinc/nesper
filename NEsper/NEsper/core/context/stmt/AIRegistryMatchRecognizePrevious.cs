///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.rowregex;

namespace com.espertech.esper.core.context.stmt
{
    public interface AIRegistryMatchRecognizePrevious : RegexExprPreviousEvalStrategy
    {
        void AssignService(int num, RegexExprPreviousEvalStrategy value);
        void DeassignService(int num);
        int AgentInstanceCount { get; }
    }
}