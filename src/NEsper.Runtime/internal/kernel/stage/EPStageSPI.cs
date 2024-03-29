///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client.stage;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public interface EPStageSPI : EPStage
	{
		StageSpecificServices StageSpecificServices { get; }
		void DestroyNoCheck();
	}
} // end of namespace
