///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowConsumerMgmtServiceImpl : NamedWindowConsumerMgmtService {
    
        public static readonly NamedWindowConsumerMgmtServiceImpl INSTANCE = new NamedWindowConsumerMgmtServiceImpl();
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private NamedWindowConsumerMgmtServiceImpl() {
        }
    
        public void AddConsumer(StatementContext statementContext, NamedWindowConsumerStreamSpec namedSpec) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Statement '" + statementContext.StatementName + " registers consumer for '" + namedSpec.WindowName + "'");
            }
        }
    
        public void Start(string statementName) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Statement '" + statementName + " starts consuming");
            }
        }
    
        public void Stop(string statementName) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Statement '" + statementName + " stop consuming");
            }
        }
    
        public void Destroy(string statementName) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Statement '" + statementName + " destroyed");
            }
        }
    
        public void RemoveReferences(string statementName) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Statement '" + statementName + " removing references");
            }
        }
    }
} // end of namespace
