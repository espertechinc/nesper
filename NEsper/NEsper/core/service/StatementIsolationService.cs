///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>Service for managing statement isolation. </summary>
    public interface StatementIsolationService : IDisposable
    {
        /// <summary>Returns an isolated service by names, or allocates a new one if none found. </summary>
        /// <param name="name">isolated service</param>
        /// <param name="optionalUnitId">the unique id assigned to the isolation unit</param>
        /// <returns>isolated service provider</returns>
        EPServiceProviderIsolated GetIsolationUnit(String name, int? optionalUnitId);
    
        /// <summary>Returns all names or currently known isolation services. </summary>
        /// <value>names</value>
        string[] IsolationUnitNames { get; }

        /// <summary>Indicates statements are moved to isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void BeginIsolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates statements are have moved to isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void CommitIsolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates statements are have not moved to isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void RollbackIsolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates statements are moved out of isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void BeginUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates statements have been moved out of isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void CommitUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates statements are not moved out of isolation. </summary>
        /// <param name="name">isolated service provider name.</param>
        /// <param name="unitId">isolated service provider number.</param>
        /// <param name="stmt">statements moved.</param>
        void RollbackUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt);
    
        /// <summary>Indicates a new statement created in an isolated service. </summary>
        /// <param name="stmtId">statement id</param>
        /// <param name="stmtName">statement name</param>
        /// <param name="isolatedServices">isolated services</param>
        void NewStatement(String stmtId, String stmtName, EPIsolationUnitServices isolatedServices);
    }
}
