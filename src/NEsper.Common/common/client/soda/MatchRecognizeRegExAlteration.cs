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
    /// Interface representing an expression for use in match-recognize.
    /// <para/>
    /// Event row regular expressions are organized into a tree-like structure
    /// with nodes representing sub-expressions.
    /// </summary>
    [Serializable]
    public class MatchRecognizeRegExAlteration
        : MatchRecognizeRegEx
    {
        public override void WriteEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var node in Children) {
                writer.Write(delimiter);
                node.WriteEPL(writer);
                delimiter = "|";
            }
        }
    }
}