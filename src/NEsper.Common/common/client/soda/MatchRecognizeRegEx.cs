///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Interface representing an expression for use in match-recognize.
    ///     <para />
    ///     Event row regular expressions are organized into a tree-like structure with nodes representing sub-expressions.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<MatchRecognizeRegEx>))]
    public abstract class MatchRecognizeRegEx
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        protected MatchRecognizeRegEx()
        {
            Children = new List<MatchRecognizeRegEx>();
        }

        /// <summary>
        ///     Returns id of expression assigned by tools.
        /// </summary>
        /// <returns>id</returns>
        public string TreeObjectName { get; set; }

        /// <summary>
        ///     Returns child nodes.
        /// </summary>
        /// <returns>child nodes</returns>
        public IList<MatchRecognizeRegEx> Children { get; set; }

        /// <summary>
        ///     Write EPL.
        /// </summary>
        /// <param name="writer">to use</param>
        public abstract void WriteEPL(TextWriter writer);
    }
} // end of namespace