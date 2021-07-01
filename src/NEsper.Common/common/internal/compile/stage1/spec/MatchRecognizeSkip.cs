///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for the skip-part of match_recognize.
    /// </summary>
    [Serializable]
    public class MatchRecognizeSkip
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="skip">enum</param>
        public MatchRecognizeSkip(MatchRecognizeSkipEnum skip)
        {
            Skip = skip;
        }

        /// <summary>Skip enum. </summary>
        /// <value>skip value</value>
        public MatchRecognizeSkipEnum Skip { get; set; }
    }
}