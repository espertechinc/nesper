///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Interface for a factory class that makes statement context specific to a statement.
    /// </summary>
    public interface StatementContextFactory
    {
        EPServicesContext StmtEngineServices { set; }

        /// <summary>
        /// Create a new statement context consisting of statement-level services.
        /// </summary>
        /// <param name="statementId">is the statement is</param>
        /// <param name="statementName">is the statement name</param>
        /// <param name="expression">is the statement expression</param>
        /// <param name="statementType">Type of the statement.</param>
        /// <param name="engineServices">is engine services</param>
        /// <param name="optAdditionalContext">addtional context to pass to the statement</param>
        /// <param name="isFireAndForget">if the statement context is for a fire-and-forget statement</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="isolationUnitServices">for isolation units</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        /// <param name="statementSpecRaw">The statement spec raw.</param>
        /// <param name="subselectNodes">The subselect nodes.</param>
        /// <param name="writeToTables">if set to <c>true</c> [write to tables].</param>
        /// <param name="statementUserObject">The statement user object.</param>
        /// <returns>
        /// statement context
        /// </returns>
        StatementContext MakeContext(
            int statementId,
            string statementName,
            string expression,
            StatementType statementType,
            EPServicesContext engineServices,
            IDictionary<string, object> optAdditionalContext,
            bool isFireAndForget,
            Attribute[] annotations,
            EPIsolationUnitServices isolationUnitServices,
            bool stateless,
            StatementSpecRaw statementSpecRaw,
            IList<ExprSubselectNode> subselectNodes,
            bool writeToTables,
            object statementUserObject);
    }
}
