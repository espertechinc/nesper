///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
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
        ExprNode CompileExpression(String expression);
    
        /// <summary>Compile expression. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        Expression CompileExpressionToSODA(String expression);
    
        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        EvalFactoryNode CompilePatternToNode(String expression);

        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        PatternExpr CompilePatternToSODA(String expression);
    
        /// <summary>Compile pattern. </summary>
        /// <param name="expression">to compile</param>
        /// <returns>compiled expression</returns>
        /// <throws>EPException if compile failed</throws>
        EPStatementObjectModel CompilePatternToSODAModel(String expression);
    
        /// <summary>Compile annotation expressions. </summary>
        /// <param name="annotationExpression">to compile</param>
        /// <returns>model representation</returns>
        AnnotationPart CompileAnnotationToSODA(String annotationExpression);
    
        /// <summary>Compile match recognize pattern expression. </summary>
        /// <param name="matchRecogPatternExpression">to compile</param>
        /// <returns>model representation</returns>
        MatchRecognizeRegEx CompileMatchRecognizePatternToSODA(String matchRecogPatternExpression);
    
        StatementSpecRaw CompileEPLToRaw(String epl);
        EPStatementObjectModel MapRawToSODA(StatementSpecRaw raw);
        StatementSpecRaw MapSODAToRaw(EPStatementObjectModel model);
        EPStatement CreateEPLStatementId(String eplStatement, String statementName, Object userObject, String statementId);
        EPStatement CreateModelStatementId(EPStatementObjectModel sodaStatement, String statementName, Object userObject, String statementId);
        EPStatement CreatePatternStatementId(String pattern, String statementName, Object userObject, String statementId);
        EPStatement CreatePreparedEPLStatementId(EPPreparedStatementImpl prepared, String statementName, Object userObject, String statementId);
        String GetStatementNameForId(String statementId);
    }
}
