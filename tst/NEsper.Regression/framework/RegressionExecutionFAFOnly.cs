///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.framework
{
	public abstract class RegressionExecutionFAFOnly : RegressionExecution
	{
		public abstract void Run(RegressionEnvironment env);

		public ISet<RegressionFlag> Flags()
		{
			return Collections.Set(RegressionFlag.FIREANDFORGET);
		}
	}
} // end of namespace
