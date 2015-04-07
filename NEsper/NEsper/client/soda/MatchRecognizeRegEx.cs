///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Interface representing an expression for use in match-recognize.
    /// <para />
    /// Event row regular expressions are organized into a tree-like structure with nodes representing sub-expressions.
    /// </summary>
    [Serializable]
    public abstract class MatchRecognizeRegEx
    {
        /// <summary>Returns id of expression assigned by tools. </summary>
        /// <value>id</value>
        public string TreeObjectName { get; set; }

        /// <summary>Ctor. </summary>
        protected MatchRecognizeRegEx()
        {
            Children = new List<MatchRecognizeRegEx>();
        }

        /// <summary>Returns child nodes. </summary>
        /// <value>child nodes</value>
        public List<MatchRecognizeRegEx> Children { get; set; }

        /// <summary>Write EPL. </summary>
        /// <param name="writer">to use</param>
        public abstract void WriteEPL(TextWriter writer);
    }
}