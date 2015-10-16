///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Service interface for match recognize.
    /// </summary>
    public interface EventRowRegexNFAViewService
    {
        void Init(EventBean[] newEvents);
        RegexExprPreviousEvalStrategy PreviousEvaluationStrategy { get; }
        void Accept(EventRowRegexNFAViewServiceVisitor visitor);
        void Stop();
    }
}
