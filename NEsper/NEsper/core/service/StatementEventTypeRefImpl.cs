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
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Service for holding references between statements and their event type use.
    /// </summary>
    public class StatementEventTypeRefImpl : StatementEventTypeRef
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReaderWriterLock _mapLock;
        private readonly Dictionary<String, ICollection<String>> _typeToStmt;
        private readonly Dictionary<String, String[]> _stmtToType;

        /// <summary>Ctor. </summary>
        public StatementEventTypeRefImpl(IReaderWriterLockManager lockManager)
        {
            _typeToStmt = new Dictionary<String, ICollection<String>>();
            _stmtToType = new Dictionary<String, String[]>();
            _mapLock = lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void AddReferences(String statementName, String[] eventTypesReferenced)
        {
            if (eventTypesReferenced.Length == 0)
            {
                return;
            }

            using (_mapLock.AcquireWriteLock())
            {
                foreach (String reference in eventTypesReferenced)
                {
                    AddReference(statementName, reference);
                }
            }
        }

        public void RemoveReferencesStatement(String statementName)
        {
            using (_mapLock.AcquireWriteLock())
            {
                var types = _stmtToType.Delete(statementName);
                if (types != null)
                {
                    foreach (String type in types)
                    {
                        RemoveReference(statementName, type);
                    }
                }
            }
        }

        public void RemoveReferencesType(String name)
        {
            using (_mapLock.AcquireWriteLock())
            {
                var statementNames = _typeToStmt.Delete(name);
                if (statementNames != null)
                {
                    foreach (String statementName in statementNames)
                    {
                        RemoveReference(statementName, name);
                    }
                }
            }
        }

        public bool IsInUse(String eventTypeName)
        {
            using (_mapLock.AcquireReadLock())
            {
                return _typeToStmt.ContainsKey(eventTypeName);
            }
        }

        public ICollection<String> GetStatementNamesForType(String eventTypeName)
        {
            using (_mapLock.AcquireReadLock())
            {
                var types = _typeToStmt.Get(eventTypeName);
                if (types == null)
                {
                    return Collections.GetEmptySet<string>();
                }
                return types.AsReadOnlyCollection();
            }
        }

        public String[] GetTypesForStatementName(String statementName)
        {
            using (_mapLock.AcquireReadLock())
            {
                var types = _stmtToType.Get(statementName);
                if (types == null)
                {
                    return new String[0];
                }
                return types;
            }
        }

        private void AddReference(String statementName, String eventTypeName)
        {
            // add to types
            var statements = _typeToStmt.Get(eventTypeName);
            if (statements == null)
            {
                statements = new HashSet<String>();
                _typeToStmt.Put(eventTypeName, statements);
            }
            statements.Add(statementName);

            // add to statements
            String[] types = _stmtToType.Get(statementName);
            if (types == null)
            {
                types = new String[]{ eventTypeName };
            }
            else
            {
                int index = CollectionUtil.FindItem(types, eventTypeName);
                if (index == -1)
                {
                    types = (String[])CollectionUtil.ArrayExpandAddSingle(types, eventTypeName);
                }
            }
            _stmtToType.Put(statementName, types);
        }

        private void RemoveReference(String statementName, String eventTypeName)
        {
            // remove from types
            ICollection<String> statements = _typeToStmt.Get(eventTypeName);
            if (statements != null)
            {
                if (!statements.Remove(statementName))
                {
                    Log.Info("Failed to find statement name '" + statementName + "' in collection");
                }

                if (statements.IsEmpty())
                {
                    _typeToStmt.Remove(eventTypeName);
                }
            }

            // remove from statements
            String[] types = _stmtToType.Get(statementName);
            if (types != null)
            {
                int index = CollectionUtil.FindItem(types, eventTypeName);
                if (index != -1)
                {
                    if (types.Length == 1)
                    {
                        _stmtToType.Remove(statementName);
                    }
                    else
                    {
                        types = (String[])CollectionUtil.ArrayShrinkRemoveSingle(types, index);
                        _stmtToType.Put(statementName, types);
                    }
                }
                else
                {
                    Log.Info("Failed to find type name '" + eventTypeName + "' in collection");
                }
            }
        }

        /// <summary>For testing, returns the mapping of event type name to statement names. </summary>
        /// <value>mapping</value>
        public Dictionary<string, ICollection<string>> TypeToStmt
        {
            get { return _typeToStmt; }
        }

        /// <summary>For testing, returns the mapping of statement names to event type names. </summary>
        /// <value>mapping</value>
        public Dictionary<string, string[]> StmtToType
        {
            get { return _stmtToType; }
        }
    }
}
