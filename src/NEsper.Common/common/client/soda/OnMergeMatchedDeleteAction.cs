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
    /// For use with on-merge clauses, deletes from a named window if matching rows are found.
    /// </summary>
    [Serializable]
    public class OnMergeMatchedDeleteAction : OnMergeMatchedAction
    {
        /// <summary>Ctor. </summary>
        /// <param name="whereClause">condition for action, or null if none required</param>
        public OnMergeMatchedDeleteAction(Expression whereClause)
        {
            WhereClause = whereClause;
        }

        /// <summary>Ctor. </summary>
        public OnMergeMatchedDeleteAction()
        {
        }

        /// <summary>Returns the action condition, or null if undefined. </summary>
        /// <value>condition</value>
        public Expression WhereClause { get; set; }

        #region OnMergeMatchedAction Members

        public void ToEPL(TextWriter writer)
        {
            writer.Write("then delete");
            if (WhereClause != null) {
                writer.Write(" where ");
                WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        #endregion OnMergeMatchedAction Members
    }
}