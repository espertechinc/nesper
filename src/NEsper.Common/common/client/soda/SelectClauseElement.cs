///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Item in a select-clause to describe individual select-clause expressions or Wildcard(s).
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<SelectClauseElement>))]
    public interface SelectClauseElement
    {
        /// <summary>Output the string rendering of the select clause element. </summary>
        /// <param name="writer">to output to</param>
        void ToEPLElement(TextWriter writer);
    }

    public static class SelectClauseElementExtension
    {
        /// <summary>
        /// Converts the element to a string.
        /// </summary>
        /// <param name="selectClauseElement">The select clause element.</param>
        /// <returns></returns>
        public static string ToEPL(this SelectClauseElement selectClauseElement)
        {
            using (var textWriter = new StringWriter()) {
                selectClauseElement.ToEPLElement(textWriter);
                return textWriter.ToString();
            }
        }
    }
}