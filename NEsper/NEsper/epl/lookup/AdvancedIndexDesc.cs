///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Defines the <see cref="AdvancedIndexDesc" />
    /// </summary>
    public class AdvancedIndexDesc
    {
        /// <summary>
        /// Defines the indexTypeName
        /// </summary>
        private readonly string _indexTypeName;

        /// <summary>
        /// Defines the indexedExpressions
        /// </summary>
        private readonly ExprNode[] _indexedExpressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedIndexDesc" /> class.
        /// </summary>
        /// <param name="indexTypeName">Name of the index type.</param>
        /// <param name="indexedExpressions">The indexed expressions.</param>
        public AdvancedIndexDesc(string indexTypeName, ExprNode[] indexedExpressions)
        {
            _indexTypeName = indexTypeName;
            _indexedExpressions = indexedExpressions;
        }

        /// <summary>
        /// Gets the index type name.
        /// </summary>
        public string IndexTypeName => _indexTypeName;

        /// <summary>
        /// Gets the indexed expressions.
        /// </summary>
        public ExprNode[] IndexedExpressions => _indexedExpressions;

        public bool EqualsAdvancedIndex(AdvancedIndexDesc that)
        {
            return IndexTypeName.Equals(that._indexTypeName) && ExprNodeUtility.DeepEquals(_indexedExpressions, that._indexedExpressions, true);
        }

        /// <summary>
        /// The ToQueryPlan
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string ToQueryPlan()
        {
            if (_indexedExpressions.Length == 0)
            {
                return _indexTypeName;
            }
            var writer = new StringWriter();
            writer.Write(_indexTypeName);
            writer.Write("(");
            ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(_indexedExpressions, writer);
            writer.Write(")");
            return writer.ToString();
        }
    }
}
