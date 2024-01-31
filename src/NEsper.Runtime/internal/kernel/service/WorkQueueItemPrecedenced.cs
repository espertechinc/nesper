///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueItemPrecedenced : IComparable<WorkQueueItemPrecedenced>
	{
		private object latchOrBean;
		private readonly int precedence;

		public WorkQueueItemPrecedenced(
			object latchOrBean,
			int precedence)
		{
			this.latchOrBean = latchOrBean;
			this.precedence = precedence;
		}

		public int Precedence => precedence;

		public object LatchOrBean {
			get => latchOrBean;
			set => this.latchOrBean = value;
		}

		public int CompareTo(WorkQueueItemPrecedenced o)
		{
			return o.precedence - precedence;
		}
	}
} // end of namespace
