///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Items within the split-stream syntax to contain a tuple of insert-into, select and where-clause.
    /// </summary>
    [Serializable]
    public class OnInsertSplitStreamItem
    {
        /// <summary>Ctor.</summary>
        public OnInsertSplitStreamItem()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="propertySelects">contained-event selections</param>
        /// <param name="propertySelectsStreamName">contained-event selection stream name</param>
        /// <param name="whereClause">where-expression or null</param>
        public OnInsertSplitStreamItem(
            InsertIntoClause insertInto,
            SelectClause selectClause,
            IList<ContainedEventSelect> propertySelects,
            string propertySelectsStreamName,
            Expression whereClause)
        {
            InsertInto = insertInto;
            SelectClause = selectClause;
            PropertySelects = propertySelects;
            PropertySelectsStreamName = propertySelectsStreamName;
            WhereClause = whereClause;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="whereClause">where-expression or null</param>
        public OnInsertSplitStreamItem(InsertIntoClause insertInto, SelectClause selectClause, Expression whereClause)
            : this(insertInto, selectClause, null, null, whereClause)
        {
        }

        /// <summary>
        /// Factory method for split-stream items.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="whereClause">where-expression or null</param>
        /// <returns>split-stream item</returns>
        public static OnInsertSplitStreamItem Create(
            InsertIntoClause insertInto,
            SelectClause selectClause,
            Expression whereClause)
        {
            return new OnInsertSplitStreamItem(insertInto, selectClause, whereClause);
        }

        /// <summary>
        /// Factory method for split-stream items.
        /// </summary>
        /// <param name="insertInto">the insert-into clause</param>
        /// <param name="selectClause">the select-clause</param>
        /// <param name="propertySelects">contained-event selects in the from-clause</param>
        /// <param name="propertySelectsStreamName">stream name for contained-event selection</param>
        /// <param name="whereClause">where-expression or null</param>
        /// <returns>split-stream item</returns>
        public static OnInsertSplitStreamItem Create(
            InsertIntoClause insertInto,
            SelectClause selectClause,
            IList<ContainedEventSelect> propertySelects,
            string propertySelectsStreamName,
            Expression whereClause)
        {
            return new OnInsertSplitStreamItem(insertInto, selectClause, propertySelects, propertySelectsStreamName, whereClause);
        }

        /// <summary>
        /// Returns the insert-into clause.
        /// </summary>
        /// <value>insert-into clause</value>
        public InsertIntoClause InsertInto { get; set; }

        /// <summary>
        /// Returns the select-clause.
        /// </summary>
        /// <value>select-clause</value>
        public SelectClause SelectClause { get; set; }

        /// <summary>
        /// Returns the optional where-clause.
        /// </summary>
        /// <value>where-clause</value>
        public Expression WhereClause { get; set; }

        /// <summary>
        /// Returns contained-event selection, if any.
        /// </summary>
        /// <value>list or null</value>
        public IList<ContainedEventSelect> PropertySelects { get; set; }

        /// <summary>
        /// Returns the stream name assigned to contained-event selects, or null
        /// </summary>
        /// <value>stream name</value>
        public string PropertySelectsStreamName { get; set; }
    }
} // end of namespace
