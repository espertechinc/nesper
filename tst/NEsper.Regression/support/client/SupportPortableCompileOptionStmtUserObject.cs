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
using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.regressionlib.support.client
{
	[Serializable]
	public class SupportPortableCompileOptionStmtUserObject // : StatementUserObjectOption
	{
		private readonly object value;

		public SupportPortableCompileOptionStmtUserObject(object value)
		{
			this.value = value;
		}

		public object GetValue(StatementUserObjectContext env)
		{
			return value;
		}

		public object GetValue()
		{
			return value;
		}

		public object Value => value;
	}
} // end of namespace
