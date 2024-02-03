///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esperio.file
{
	public class FileSinkForge : DataFlowOperatorForge
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[DataFlowOpParameter] private ExprNode file;

		[DataFlowOpParameter] private ExprNode append;

		private EventType eventType;

		public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
		{
			if (context.InputPorts.Count != 1) {
				throw new EPException(GetType().Name + " expected a single input port");
			}

			eventType = context.InputPorts[0].TypeDesc.EventType;
			if (eventType == null) {
				throw new EPException("No event type defined for input port");
			}

			file = DataFlowParameterValidation.Validate("file", file, typeof(string), context);
			append = DataFlowParameterValidation.Validate("append", append, typeof(bool), context);
			return null;
		}

		public CodegenExpression Make(CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			return new SAIFFInitializeBuilder(typeof(FileSinkFactory), GetType(), "factory", parent, symbols, classScope)
				.Exprnode("file", file)
				.Exprnode("append", append)
				.Eventtype("eventType", eventType)
				.Build();
		}
	}
} // end of namespace
