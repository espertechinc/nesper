///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.service
{
    /// <summary>Administrative SPI. </summary>
    public interface EPAdministratorSPI : EPAdministrator, IDisposable
    {
        /// <summary>Compile expression. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        ExprNode CompileExpression(string expression);
    
        /// <summary>Compile expression. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        Expression CompileExpressionToSODA(string expression);
    
        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        EvalFactoryNode CompilePatternToNode(string expression);

        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        PatternExpr CompilePatternToSODA(string expression);
    
        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        EPStatementObjectModel CompilePatternToSODAModel(string expression);
    
        /// <summary>Compile annotation expressions. </summary>
        /// <param name="annotationExpression">to compile</param>
        /// <returns>model representation</returns>
        AnnotationPart CompileAnnotationToSODA(string annotationExpression);
    
        /// <summary>Compile match recognize pattern expression. </summary>
        /// <param name="matchRecogPatternExpression">to compile</param>
        /// <returns>model representation</returns>
        MatchRecognizeRegEx CompileMatchRecognizePatternToSODA(string matchRecogPatternExpression);
    
        StatementSpecRaw CompileEPLToRaw(string epl);
        EPStatementObjectModel MapRawToSODA(StatementSpecRaw raw);
        StatementSpecRaw MapSODAToRaw(EPStatementObjectModel model);
        EPStatement CreateEPLStatementId(string eplStatement, string statementName, object userObject, int statementId);
        EPStatement CreateModelStatementId(EPStatementObjectModel sodaStatement, string statementName, object userObject, int statementId);
        EPStatement CreatePatternStatementId(string pattern, string statementName, object userObject, int statementId);
        EPStatement CreatePreparedEPLStatementId(EPPreparedStatementImpl prepared, string statementName, object userObject, int statementId);
        string GetStatementNameForId(int statementId);
    }
}
