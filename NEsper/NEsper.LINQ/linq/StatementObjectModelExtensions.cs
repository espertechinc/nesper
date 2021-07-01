///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.runtime.client.linq
{
    public static class StatementObjectModelExtensions
    {
        /// <summary>
        /// Creates a shallow copy of the object model.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static EPStatementObjectModel ShallowCopy(this EPStatementObjectModel source)
        {
            return new EPStatementObjectModel
            {
                Annotations = source.Annotations,
                InsertInto = source.InsertInto,
                SelectClause = source.SelectClause,
                FromClause = source.FromClause,
                WhereClause = source.WhereClause,
                GroupByClause = source.GroupByClause,
                HavingClause = source.HavingClause,
                OutputLimitClause = source.OutputLimitClause,
                OrderByClause = source.OrderByClause,
                CreateVariable = source.CreateVariable,
                CreateWindow = source.CreateWindow,
                OnExpr = source.OnExpr,
                RowLimitClause = source.RowLimitClause
            };
        }

        public static void MakeIterableUnbound(this EPStatementObjectModel objectModel)
        {
            objectModel.Annotations = new List<AnnotationPart>()
            {
                new AnnotationPart("IterableUnbound")
            };
        }
    }
}