///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Items within the split-stream syntax to contain a tuple of insert-into, select
    /// and where-clause.
    /// </summary>
    [Serializable]
    public class OnInsertSplitStreamItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnInsertSplitStreamItem"/> class.
        /// </summary>
        public OnInsertSplitStreamItem()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="whereClause">where-expression or null</param>
        public OnInsertSplitStreamItem(InsertIntoClause insertInto, SelectClause selectClause, Expression whereClause)
        {
            InsertInto = insertInto;
            SelectClause = selectClause;
            WhereClause = whereClause;
        }

        /// <summary>
        /// Factory method for split-stream items.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="whereClause">where-expression or null</param>
        /// <returns>
        /// split-stream item
        /// </returns>
        public static OnInsertSplitStreamItem Create(InsertIntoClause insertInto,
                                                     SelectClause selectClause,
                                                     Expression whereClause)
        {
            return new OnInsertSplitStreamItem(insertInto, selectClause, whereClause);
        }

        /// <summary>
        /// Gets or sets the insert-into clause.
        /// </summary>
        /// <returns>
        /// insert-into clause
        /// </returns>
        public InsertIntoClause InsertInto { get; set; }

        /// <summary>
        /// Gets or sets the select-clause.
        /// </summary>
        /// <returns>
        /// select-clause
        /// </returns>
        public SelectClause SelectClause { get; set; }

        /// <summary>
        /// Returns the optional where-clause.
        /// </summary>
        /// <returns>
        /// where-clause
        /// </returns>
        public Expression WhereClause { get; set; }
    }
}
