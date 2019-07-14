///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
                Assert.AreEqual(deploymentId, @event.ContextDeploymentId);
                Assert.AreEqual(contextName, @event.ContextName);
                Assert.AreEqual(type, @event.GetType());
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
                Assert.AreEqual("default", @event.RuntimeURI);
                Assert.AreEqual(contextDeploymentId, @event.ContextDeploymentId);
                Assert.AreEqual(contextName, @event.ContextName);
                Assert.AreEqual(type, @event.GetType());
                if (@event is ContextStateEventContextStatementAdded) {
                    var added = (ContextStateEventContextStatementAdded) @event;
                    Assert.AreEqual(statementDeploymentId, added.StatementDeploymentId);
                    Assert.AreEqual(statementName, added.StatementName);
                }
                else if (@event is ContextStateEventContextStatementRemoved) {
                    var removed = (ContextStateEventContextStatementRemoved) @event;
                    Assert.AreEqual(statementDeploymentId, removed.StatementDeploymentId);
                    Assert.AreEqual(statementName, removed.StatementName);
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
                Assert.AreEqual("default", @event.RuntimeURI);
                Assert.AreEqual(contextDeploymentId, @event.ContextDeploymentId);
                Assert.AreEqual(contextName, @event.ContextName);
                Assert.AreEqual(type, @event.GetType());
                var partition = (ContextStateEventContextPartition) @event;
                if (partition is ContextStateEventContextPartitionAllocated) {
                    var allocated = (ContextStateEventContextPartitionAllocated) @event;
                    if (allocated.Identifier is ContextPartitionIdentifierInitiatedTerminated) {
                        var ident = (ContextPartitionIdentifierInitiatedTerminated) allocated.Identifier;
                        Assert.IsNotNull(ident.Properties.Get("s0"));
                    }
                    else if (allocated.Identifier is ContextPartitionIdentifierNested) {
                        var nested = (ContextPartitionIdentifierNested) allocated.Identifier;
                        var ident = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[1];
                        Assert.IsNotNull(ident.Properties.Get("s0"));
                    }
                }
            };
        }
    }
} // end of namespace