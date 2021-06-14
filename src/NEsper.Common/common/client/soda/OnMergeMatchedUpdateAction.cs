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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// For use with on-merge clauses, updates rows in a named window if matching rows are found.
    /// </summary>
    [Serializable]
    public class OnMergeMatchedUpdateAction : OnMergeMatchedAction
    {
        /// <summary>Ctor. </summary>
        public OnMergeMatchedUpdateAction()
        {
            Assignments = Collections.GetEmptyList<Assignment>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="assignments">assignments of values to columns</param>
        /// <param name="whereClause">optional condition or null</param>
        public OnMergeMatchedUpdateAction(
            IList<Assignment> assignments,
            Expression whereClause)
        {
            Assignments = assignments;
            WhereClause = whereClause;
        }

        /// <summary>Returns the action condition, or null if undefined. </summary>
        /// <value>condition</value>
        public Expression WhereClause { get; set; }

        /// <summary>Returns the assignments to execute against any rows found in a named window </summary>
        /// <value>assignments</value>
        public IList<Assignment> Assignments { get; set; }

        public void ToEPL(TextWriter writer)
        {
            writer.Write("then update ");
            UpdateClause.RenderEPLAssignments(writer, Assignments);
            if (WhereClause != null)
            {
                writer.Write(" where ");
                WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
}