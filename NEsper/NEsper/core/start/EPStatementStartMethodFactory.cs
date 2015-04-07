///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodFactory
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementSpec">
        /// is a container for the definition of all statement constructs that
        /// may have been used in the statement, i.e. if defines the select clauses,
        /// insert into, outer joins etc.
        /// </param>
        /// <returns></returns>
        public static EPStatementStartMethod MakeStartMethod(StatementSpecCompiled statementSpec)
        {
            if (statementSpec.UpdateSpec != null)
            {
                return new EPStatementStartMethodUpdate(statementSpec);
            }
            if (statementSpec.OnTriggerDesc != null)
            {
                return new EPStatementStartMethodOnTrigger(statementSpec);
            }
            else if (statementSpec.CreateWindowDesc != null)
            {
                return new EPStatementStartMethodCreateWindow(statementSpec);
            }
            else if (statementSpec.CreateIndexDesc != null)
            {
                return new EPStatementStartMethodCreateIndex(statementSpec);
            }
            else if (statementSpec.CreateGraphDesc != null)
            {
                return new EPStatementStartMethodCreateGraph(statementSpec);
            }
            else if (statementSpec.CreateSchemaDesc != null)
            {
                return new EPStatementStartMethodCreateSchema(statementSpec);
            }
            else if (statementSpec.CreateVariableDesc != null)
            {
                return new EPStatementStartMethodCreateVariable(statementSpec);
            }
            else if (statementSpec.CreateTableDesc != null)
            {
                return new EPStatementStartMethodCreateTable(statementSpec);
            }
            else if (statementSpec.ContextDesc != null)
            {
                return new EPStatementStartMethodCreateContext(statementSpec);
            }
            else if (statementSpec.CreateExpressionDesc != null)
            {
                return new EPStatementStartMethodCreateExpression(statementSpec);
            }
            else
            {
                return new EPStatementStartMethodSelect(statementSpec);
            }
        }
    }
}