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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.view;
using com.espertech.esper.util;

using NUnit.Framework;

using SupportEventAdapterService = com.espertech.esper.support.events.SupportEventAdapterService;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
	public class TestSelectExprProcessorFactory 
	{
	    private readonly StatementResultService _statementResultService = new StatementResultServiceImpl(
            "name", null, null, new ThreadingServiceImpl(new ConfigurationEngineDefaults.Threading()));
	    private readonly SelectExprEventTypeRegistry _selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(
            "abc", new StatementEventTypeRefImpl());

        [Test]
	    public void TestGetProcessorInvalid()
	    {
	        var selectionList = new SelectClauseElementCompiled[2];
	        var identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
	        var mathNode = SupportExprNodeFactory.MakeMathNode();
	        selectionList[0] = new SelectClauseExprCompiledSpec(identNode, "result", "result", false);
	        selectionList[1] = new SelectClauseExprCompiledSpec(mathNode, "result", "result", false);

	        try
	        {
	            SelectExprProcessorFactory.GetProcessor(
	                Collections.GetEmptyList<int>(), selectionList, false, null, null, null,
	                new SupportStreamTypeSvc3Stream(), null, null, null, null, null, null, null, null, null, null, null,
	                1, null, null, null, new Configuration(), null, null, null, null);
	            Assert.Fail();
	        }
	        catch (ExprValidationException)
	        {
	            // Expected
	        }
	    }

        [Test]
	    public void TestGetProcessorWildcard()
	    {
	        var selectionList = new SelectClauseElementCompiled[] {new SelectClauseElementWildcard()};
            var processor = SelectExprProcessorFactory.GetProcessor(
                Collections.GetEmptyList<int>(), selectionList, false, null, null, null,
                new SupportStreamTypeSvc3Stream(), SupportEventAdapterService.Service, _statementResultService, null,
                _selectExprEventTypeRegistry, null, null, null, null,
                new TableServiceImpl(), null, null, 1, null, null, null, new Configuration(), null, null, null, null);
	        Assert.IsTrue(processor is SelectExprResultProcessor);
	    }

        [Test]
	    public void TestGetProcessorValid()
	    {
	        var selectionList = new SelectClauseElementCompiled[1];
	        var identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
	        selectionList[0] = new SelectClauseExprCompiledSpec(identNode, "result", null, false);
	        var statementContext = SupportStatementContextFactory.MakeContext();
            var processor = SelectExprProcessorFactory.GetProcessor(
                Collections.GetEmptyList<int>(), selectionList, false, null, null, null,
                new SupportStreamTypeSvc3Stream(), SupportEventAdapterService.Service, _statementResultService, null,
                _selectExprEventTypeRegistry,
                statementContext.MethodResolutionService, null, null, null, null, null, null, 1, null, null, null,
                new Configuration(), null, null, null, null);
	        Assert.IsTrue(processor != null);
	    }

        [Test]
	    public void TestVerifyNameUniqueness()
	    {
	        // try valid case
	        var elements = new SelectClauseElementCompiled[4];
	        elements[0] = new SelectClauseExprCompiledSpec(null, "xx", null, false);
	        elements[1] = new SelectClauseExprCompiledSpec(null, "yy", null, false);
	        elements[2] = new SelectClauseStreamCompiledSpec("win", null);
	        elements[3] = new SelectClauseStreamCompiledSpec("s2", "abc");

	        SelectExprProcessorFactory.VerifyNameUniqueness(elements);

	        // try invalid case
	        elements = (SelectClauseElementCompiled[]) CollectionUtil.ArrayExpandAddSingle(elements, new SelectClauseExprCompiledSpec(null, "yy", null, false));
	        try
	        {
	            SelectExprProcessorFactory.VerifyNameUniqueness(elements);
	            Assert.Fail();
	        }
	        catch (ExprValidationException)
	        {
	            // expected
	        }

	        // try invalid case
	        elements = new SelectClauseElementCompiled[2];
	        elements[0] = new SelectClauseExprCompiledSpec(null, "abc", null, false);
	        elements[1] = new SelectClauseStreamCompiledSpec("s0", "abc");
	        try
	        {
	            SelectExprProcessorFactory.VerifyNameUniqueness(elements);
	            Assert.Fail();
	        }
	        catch (ExprValidationException)
	        {
	            // expected
	        }
	    }
	}
} // end of namespace
