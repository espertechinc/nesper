///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.createclass
{
	public class StatementAgentInstanceFactoryCreateClass : StatementAgentInstanceFactory
	{
		private string _className;
		private Viewable _viewable;

		public EventType StatementEventType {
			set => _viewable = new ViewableDefaultImpl(value);
			get => _viewable.EventType;
		}

		public string ClassName {
			set => _className = value;
			get => _className;
		}

		public void StatementCreate(StatementContext statementContext)
		{
		}

		public void StatementDestroy(StatementContext statementContext)
		{
		}

		public StatementAgentInstanceFactoryResult NewContext(
			AgentInstanceContext agentInstanceContext,
			bool isRecoveringResilient)
		{
			return new StatementAgentInstanceFactoryCreateClassResult(_viewable, AgentInstanceMgmtCallbackNoAction.INSTANCE, agentInstanceContext);
		}

		public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

		public StatementAgentInstanceLock ObtainAgentInstanceLock(
			StatementContext statementContext,
			int agentInstanceId)
		{
			return AgentInstanceUtil.NewLock(statementContext);
		}
	}
} // end of namespace
