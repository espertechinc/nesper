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
    ///     Dot-expression item representing an identifier without parameters.
    /// </summary>
    [Serializable]
    public class DotExpressionItemName : DotExpressionItem
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public DotExpressionItemName()
        {
        }

        /// <summary>
        ///     Constructor with name.
        /// </summary>
        /// <param name="name">name</param>
        public DotExpressionItemName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>
        ///     Render to EPL.
        /// </summary>
        /// <param name="writer">writer to output to</param>
        public override void RenderItem(TextWriter writer)
        {
            writer.Write(Name);
        }
    }
}