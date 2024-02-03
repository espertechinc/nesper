///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportContextListenUtil
    {
        public static Consumer<ContextStateEvent> EventContext(
            string deploymentId,
            string contextName,
            Type type)
        {
            return @event => {
                ClassicAssert.AreEqual(deploymentId, @event.ContextDeploymentId);
                ClassicAssert.AreEqual(contextName, @event.ContextName);
                ClassicAssert.AreEqual(type, @event.GetType());
            };
        }

        public static Consumer<ContextStateEvent> EventContextWStmt(
            string contextDeploymentId,
            string contextName,
            Type type,
            string statementDeploymentId,
            string statementName)
        {
            return @event => {
                ClassicAssert.AreEqual("default", @event.RuntimeURI);
                ClassicAssert.AreEqual(contextDeploymentId, @event.ContextDeploymentId);
                ClassicAssert.AreEqual(contextName, @event.ContextName);
                ClassicAssert.AreEqual(type, @event.GetType());
                if (@event is ContextStateEventContextStatementAdded) {
                    var added = (ContextStateEventContextStatementAdded) @event;
                    ClassicAssert.AreEqual(statementDeploymentId, added.StatementDeploymentId);
                    ClassicAssert.AreEqual(statementName, added.StatementName);
                }
                else if (@event is ContextStateEventContextStatementRemoved) {
                    var removed = (ContextStateEventContextStatementRemoved) @event;
                    ClassicAssert.AreEqual(statementDeploymentId, removed.StatementDeploymentId);
                    ClassicAssert.AreEqual(statementName, removed.StatementName);
                }
                else {
                    Assert.Fail();
                }
            };
        }

        public static Consumer<ContextStateEvent> EventPartitionInitTerm(
            string contextDeploymentId,
            string contextName,
            Type type)
        {
            return @event => {
                ClassicAssert.AreEqual("default", @event.RuntimeURI);
                ClassicAssert.AreEqual(contextDeploymentId, @event.ContextDeploymentId);
                ClassicAssert.AreEqual(contextName, @event.ContextName);
                ClassicAssert.AreEqual(type, @event.GetType());
                var partition = (ContextStateEventContextPartition) @event;
                if (partition is ContextStateEventContextPartitionAllocated) {
                    var allocated = (ContextStateEventContextPartitionAllocated) @event;
                    if (allocated.Identifier is ContextPartitionIdentifierInitiatedTerminated) {
                        var ident = (ContextPartitionIdentifierInitiatedTerminated) allocated.Identifier;
                        ClassicAssert.IsNotNull(ident.Properties.Get("S0"));
                    }
                    else if (allocated.Identifier is ContextPartitionIdentifierNested) {
                        var nested = (ContextPartitionIdentifierNested) allocated.Identifier;
                        var ident = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[1];
                        ClassicAssert.IsNotNull(ident.Properties.Get("S0"));
                    }
                }
            };
        }
    }
} // end of namespace