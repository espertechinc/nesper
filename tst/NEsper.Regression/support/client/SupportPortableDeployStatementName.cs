///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client.option;

namespace com.espertech.esper.regressionlib.support.client
{
	[Serializable]
	public class SupportPortableDeployStatementName // : StatementNameRuntimeOption
	{
		private readonly string name;

		public SupportPortableDeployStatementName(string name)
		{
			this.name = name;
		}

		public string GetStatementName(StatementNameRuntimeContext env)
		{
			return name;
		}
	}
} // end of namespace
