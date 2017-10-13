///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.soda;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Administrative interface to the event stream processing engine. Includes methods
    /// to create patterns and EPL statements.
    /// </summary>
    public interface EPAdministrator
    {
        /// <summary>
        /// Returns deployment administrative services.
        /// </summary>
        /// <returns>deployment administration</returns>
        EPDeploymentAdmin DeploymentAdmin { get; }

        /// <summary>
        /// Create and starts an event pattern statement for the expressing string passed.
        /// <para/>
        /// The engine assigns a unique name to the statement.
        /// </summary>
        /// <param name="onExpression">must follow the documented syntax for pattern statements</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreatePattern(String onExpression);

        /// <summary>
        /// Creates and starts an EPL statement.
        /// <para/>
        /// The engine assigns a unique name to the statement. The returned statement is in
        /// started state.
        /// </summary>
        /// <param name="eplStatement">is the query language statement</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreateEPL(String eplStatement);

        /// <summary>
        /// Create and starts an event pattern statement for the expressing string passed
        /// and assign the name passed.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// </summary>
        /// <param name="onExpression">must follow the documented syntax for pattern statements</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreatePattern(String onExpression, String statementName);

        /// <summary>
        /// Create and starts an event pattern statement for the expressing string passed
        /// and assign the name passed.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="onExpression">must follow the documented syntax for pattern statements</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreatePattern(String onExpression, String statementName, Object userObject);

        /// <summary>
        /// Create and starts an event pattern statement for the expressing string passed
        /// and assign the name passed.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="onExpression">must follow the documented syntax for pattern statements</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreatePattern(String onExpression, Object userObject);

        /// <summary>
        /// Create and starts an EPL statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// </summary>
        /// <param name="eplStatement">is the query language statement</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreateEPL(String eplStatement, String statementName);

        /// <summary>
        /// Create and starts an EPL statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="eplStatement">is the query language statement</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreateEPL(String eplStatement, String statementName, Object userObject);

        /// <summary>
        /// Create and starts an EPL statement.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="eplStatement">is the query language statement</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement CreateEPL(String eplStatement, Object userObject);

        /// <summary>
        /// Creates and starts an EPL statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// </summary>
        /// <param name="sodaStatement">is the statement object model</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement Create(EPStatementObjectModel sodaStatement, String statementName);

        /// <summary>
        /// Creates and starts an EPL statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="sodaStatement">is the statement object model</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement Create(EPStatementObjectModel sodaStatement, String statementName, Object userObject);

        /// <summary>
        /// Creates and starts an EPL statement.
        /// </summary>
        /// <param name="sodaStatement">is the statement object model</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement Create(EPStatementObjectModel sodaStatement);

        /// <summary>
        /// Compiles a given EPL into an object model representation of the query.
        /// </summary>
        /// <param name="eplExpression">is the statement text to compile</param>
        /// <returns>
        /// object model of statement
        /// </returns>
        /// <throws>EPException indicates compilation errors.</throws>
        EPStatementObjectModel CompileEPL(String eplExpression);

        /// <summary>
        /// Prepares a statement for the given EPL, which can include substitution
        /// parameters marked via question mark '?'.
        /// </summary>
        /// <param name="eplExpression">is the statement text to prepare</param>
        /// <returns>
        /// prepared statement
        /// </returns>
        /// <throws>EPException indicates compilation errors.</throws>
        EPPreparedStatement PrepareEPL(String eplExpression);

        /// <summary>
        /// Prepares a statement for the given pattern, which can include substitution
        /// parameters marked via question mark '?'.
        /// </summary>
        /// <param name="patternExpression">is the statement text to prepare</param>
        /// <returns>
        /// prepared statement
        /// </returns>
        /// <throws>EPException indicates compilation errors.</throws>
        EPPreparedStatement PreparePattern(String patternExpression);

        /// <summary>
        /// Creates and starts a prepared statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// </summary>
        /// <param name="prepared">is the prepared statement for which all substitution values have been provided</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the prepared statement was not valid</throws>
        EPStatement Create(EPPreparedStatement prepared, String statementName);

        /// <summary>
        /// Creates and starts a prepared statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="prepared">is the prepared statement for which all substitution values have been provided</param>
        /// <param name="statementName">is the name to assign to the statement for use in managing the statement</param>
        /// <param name="userObject">is the application-defined user object</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the prepared statement was not valid</throws>
        EPStatement Create(EPPreparedStatement prepared, String statementName, Object userObject);

        /// <summary>
        /// Creates and starts a prepared statement.
        /// </summary>
        /// <param name="prepared">is the prepared statement for which all substitution values have been provided</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to
        /// </returns>
        /// <throws>EPException when the expression was not valid</throws>
        EPStatement Create(EPPreparedStatement prepared);

        /// <summary>
        /// Returns the statement by the given statement name. Returns null if a statement
        /// of that name has not been created, or if the statement by that name has been
        /// destroyed.
        /// </summary>
        /// <param name="name">is the statement name to return the statement for</param>
        /// <returns>
        /// statement for the given name, or null if no such started or stopped statement
        /// exists
        /// </returns>
        EPStatement GetStatement(String name);

        /// <summary>
        /// Returns the statement names of all started and stopped statements.
        /// <para/>
        /// This excludes the name of destroyed statements.
        /// </summary>
        /// <returns>
        /// statement names
        /// </returns>
        IList<string> StatementNames { get; }

        /// <summary>
        /// Starts all statements that are in stopped state. Statements in started state are
        /// not affected by this method.
        /// </summary>
        /// <throws>EPException when an error occured starting statements.</throws>
        void StartAllStatements();

        /// <summary>
        /// Stops all statements that are in started state. Statements in stopped state are
        /// not affected by this method.
        /// </summary>
        /// <throws>EPException when an error occured stopping statements</throws>
        void StopAllStatements();

        /// <summary>
        /// Stops and destroys all statements.
        /// </summary>
        /// <throws>EPException when an error occured stopping or destroying statements</throws>
        void DestroyAllStatements();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        ConfigurationOperations Configuration { get; }

        /// <summary>
        /// Returns the administrative interface for context partitions.
        /// </summary>
        EPContextPartitionAdmin ContextPartitionAdmin { get; }
    }
}
