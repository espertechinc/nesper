///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.function;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public interface EPStageServiceSPI : EPStageService
	{
		void Clear();

		void RecoverStage(
			string stageURI,
			int stageId,
			long stageCurrentTime);

		void RecoverDeployment(
			string stageUri,
			DeploymentInternal deployment);

		void RecoveredStageInitialize(Supplier<ICollection<EventType>> availableTypes);
		
		bool IsEmpty();
		
		IDictionary<string, EPStageImpl> Stages { get; }
	
		void Destroy();
	}
} // end of namespace
