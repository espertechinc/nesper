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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Dot-expression item representing an array operation.
    /// </summary>
    /// 
    public class DotExpressionItemArray : DotExpressionItem
    {
        private IList<Expression> _indexes;

        /// <summary>
        /// Gets or set the indexes.
        /// </summary>
        public IList<Expression> Indexes {
            get => _indexes;
            set => _indexes = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DotExpressionItemArray()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="indexes">array of index expressions.</param>
        public DotExpressionItemArray(IList<Expression> indexes)
        {
            _indexes = indexes;
        }

        public override void RenderItem(TextWriter writer)
        {
            writer.Write('[');
            var delimiter = "";
            foreach (var index in _indexes) {
                writer.Write(delimiter);
                delimiter = ",";
                index.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(']');
        }
    }
}