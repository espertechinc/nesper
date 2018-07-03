///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Service for holding references between statements and their variable use.
    /// </summary>
    public class StatementVariableRefImpl : StatementVariableRef
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReaderWriterLock _mapLock;
        private readonly IDictionary<String, ICollection<String>> _variableToStmt;
        private readonly IDictionary<String, ICollection<String>> _stmtToVariable;
        private readonly VariableService _variableService;
        private readonly TableService _tableService;
        private readonly NamedWindowMgmtService _namedWindowMgmtService;
        private readonly ICollection<String> _configuredVariables;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variableService">variables</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="namedWindowMgmtService">The named window MGMT service.</param>
        /// <param name="lockManager">The lock manager.</param>
        public StatementVariableRefImpl(
            VariableService variableService, 
            TableService tableService, 
            NamedWindowMgmtService namedWindowMgmtService,
            IReaderWriterLockManager lockManager)
        {
            _variableToStmt = new Dictionary<String, ICollection<String>>().WithNullSupport();
            _stmtToVariable = new Dictionary<String, ICollection<String>>().WithNullSupport();
            _mapLock = lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _variableService = variableService;
            _tableService = tableService;
            _namedWindowMgmtService = namedWindowMgmtService;

            _configuredVariables = new HashSet<String>();
            foreach (var entry in variableService.VariableReadersNonCP)
            {
                _configuredVariables.Add(entry.Key);
            }
        }

        public void AddConfiguredVariable(String variableName)
        {
            _configuredVariables.Add(variableName);
        }

        public void RemoveConfiguredVariable(String variableName)
        {
            _configuredVariables.Remove(variableName);
        }

        public void AddReferences(String statementName, ICollection<String> variablesReferenced, ExprTableAccessNode[] tableNodes)
        {
            using (_mapLock.AcquireWriteLock())
            {
                if (variablesReferenced != null)
                {
                    foreach (var reference in variablesReferenced)
                    {
                        AddReference(statementName, reference);
                    }
                }
                if (tableNodes != null)
                {
                    foreach (var tableNode in tableNodes)
                    {
                        AddReference(statementName, tableNode.TableName);
                    }
                }
            }
        }

        public void AddReferences(String statementName, String variableReferenced)
        {
            using (_mapLock.AcquireWriteLock())
            {
                AddReference(statementName, variableReferenced);
            }
        }

        public void RemoveReferencesStatement(String statementName)
        {
            using (_mapLock.AcquireWriteLock())
            {
                var variables = _stmtToVariable.Delete(statementName);
                if (variables != null)
                {
                    foreach (var variable in variables)
                    {
                        RemoveReference(statementName, variable);
                    }
                }
            }
        }

        public void RemoveReferencesVariable(String name)
        {
            using (_mapLock.AcquireWriteLock())
            {
                var statementNames = _variableToStmt.Delete(name);
                if (statementNames != null)
                {
                    foreach (var statementName in statementNames)
                    {
                        RemoveReference(statementName, name);
                    }
                }
            }
        }

        public bool IsInUse(String variable)
        {
            using (_mapLock.AcquireReadLock())
            {
                return _variableToStmt.ContainsKey(variable);
            }
        }

        public ICollection<String> GetStatementNamesForVar(String variableName)
        {
            using (_mapLock.AcquireReadLock())
            {
                var variables = _variableToStmt.Get(variableName);
                if (variables == null)
                {
                    return new string[0];
                }
                return variables.AsReadOnlyCollection();
            }
        }

        private void AddReference(String statementName, String variableName)
        {
            // add to variables
            var statements = _variableToStmt.Get(variableName);
            if (statements == null)
            {
                statements = new HashSet<String>();
                _variableToStmt.Put(variableName, statements);
            }
            statements.Add(statementName);

            // add to statements
            var variables = _stmtToVariable.Get(statementName);
            if (variables == null)
            {
                variables = new HashSet<String>();
                _stmtToVariable.Put(statementName, variables);
            }

            variables.Add(variableName);
        }

        private void RemoveReference(String statementName, String variableName)
        {
            // remove from variables
            var statements = _variableToStmt.Get(variableName);
            if (statements != null)
            {
                if (!statements.Remove(statementName))
                {
                    Log.Info("Failed to find statement name '" + statementName + "' in collection");
                }

                if (statements.IsEmpty())
                {
                    _variableToStmt.Remove(variableName);

                    if (!_configuredVariables.Contains(variableName))
                    {
                        _variableService.RemoveVariableIfFound(variableName);
                        _tableService.RemoveTableIfFound(variableName);
                        _namedWindowMgmtService.RemoveNamedWindowIfFound(variableName);
                    }
                }
            }

            // remove from statements
            var variables = _stmtToVariable.Get(statementName);
            if (variables != null)
            {
                if (!variables.Remove(variableName))
                {
                    Log.Info("Failed to find variable '" + variableName + "' in collection");
                }

                if (variables.IsEmpty())
                {
                    _stmtToVariable.Remove(statementName);
                }
            }
        }

        /// <summary>For testing, returns the mapping of variable name to statement names. </summary>
        /// <value>mapping</value>
        protected IDictionary<string, ICollection<string>> VariableToStmt
        {
            get { return _variableToStmt; }
        }

        /// <summary>For testing, returns the mapping of statement names to variable names. </summary>
        /// <value>mapping</value>
        protected IDictionary<string, ICollection<string>> StmtToVariable
        {
            get { return _stmtToVariable; }
        }
    }
}
