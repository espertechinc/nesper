///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    /// Dot-expression item is for use in "root_expression.dot_expression".
    /// * Root-expressions can be name or call.
    /// * Dot-expressions can be name, call or array.
    /// * Name is an identifier without parameters.
    /// * Call is an identifier with zero or more parameters.
    /// * Array is an index expression.
    /// <para>
    ///     Each item represent an individual chain item and may either be a name, or a call or an array.
    /// </para>
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(JsonConverterAbstract<DotExpressionItem>))]
    public abstract class DotExpressionItem
    {
        /// <summary>Ctor. </summary>
        public DotExpressionItem()
        {
        }

        /// <summary>
        /// Render to EPL.
        /// </summary>
        /// <param name="writer">writer to output to</param>
        public abstract void RenderItem(TextWriter writer);

        /// <summary>RenderAny to EPL. </summary>
        /// <param name="chain">chain to render</param>
        /// <param name="writer">writer to output to</param>
        /// <param name="prefixDot">indicator whether to prefix with "."</param>
        protected internal static void Render(
            IList<DotExpressionItem> chain,
            TextWriter writer,
            bool prefixDot)
        {
            var delimiterOuter = prefixDot ? "." : "";
            foreach (var item in chain) {
                if (!(item is DotExpressionItemArray)) {
                    writer.Write(delimiterOuter);
                }

                item.RenderItem(writer);
                delimiterOuter = ".";
            }
        }
    }
}