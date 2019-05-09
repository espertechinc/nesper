///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// For use in match recognize pattern expression as a placeholder to represent its child nodes.
    /// </summary>
    [Serializable]
    public class MatchRecognizeRegExPlaceholder
        : MatchRecognizeRegEx
    {
        public override void WriteEPL(TextWriter writer)
        {
            if ((Children == null) || (Children.Count == 0)) {
                return;
            }

            Children[0].WriteEPL(writer);
        }
    }
}