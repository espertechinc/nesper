///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esperio.file
{
	[DataFlowOpProvideSignal]
	public class FileSourceForge : DataFlowOperatorForge
	{

		[DataFlowOpParameter] private ExprNode file;

		[DataFlowOpParameter] private ExprNode hasHeaderLine;

		[DataFlowOpParameter] private ExprNode hasTitleLine;

		[DataFlowOpParameter] private IDictionary<string, object> adapterInputSource;

		[DataFlowOpParameter] private ExprNode numLoops;

		[DataFlowOpParameter] private string[] propertyNames;

		[DataFlowOpParameter] private ExprNode format;

		[DataFlowOpParameter] private ExprNode propertyNameLine;

		[DataFlowOpParameter] private ExprNode propertyNameFile;

		[DataFlowOpParameter] private ExprNode dateFormat;

		private EventType _outputEventType;

		private EventType[] _outputPortTypes;

		public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
		{
			_outputEventType = context.OutputPorts[0].OptionalDeclaredType != null ? context.OutputPorts[0].OptionalDeclaredType.EventType : null;
			if (_outputEventType == null) {
				throw new ExprValidationException("No event type provided for output, please provide an event type name");
			}

			_outputPortTypes = new EventType[context.OutputPorts.Count];
			foreach (var entry in context.OutputPorts) {
				_outputPortTypes[entry.Key] = entry.Value.OptionalDeclaredType.EventType;
			}

			file = DataFlowParameterValidation.Validate("file", file, typeof(string), context);
			hasHeaderLine = DataFlowParameterValidation.Validate("hasHeaderLine", hasHeaderLine, typeof(bool), context);
			hasTitleLine = DataFlowParameterValidation.Validate("hasTitleLine", hasTitleLine, typeof(bool), context);
			numLoops = DataFlowParameterValidation.Validate("numLoops", numLoops, typeof(int?), context);
			format = DataFlowParameterValidation.Validate("format", format, typeof(string), context);
			propertyNameLine = DataFlowParameterValidation.Validate("propertyNameLine", propertyNameLine, typeof(string), context);
			propertyNameFile = DataFlowParameterValidation.Validate("propertyNameFile", propertyNameFile, typeof(string), context);
			dateFormat = DataFlowParameterValidation.Validate("dateFormat", dateFormat, typeof(string), context);
			return null;
		}

		public CodegenExpression Make(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			return new SAIFFInitializeBuilder(typeof(FileSourceFactory), GetType(), "factory", parent, symbols, classScope)
				.Exprnode("file", file)
				.Exprnode("hasHeaderLine", hasHeaderLine)
				.Exprnode("hasTitleLine", hasTitleLine)
				.Exprnode("numLoops", numLoops)
				.Constant("propertyNames", propertyNames)
				.Exprnode("format", format)
				.Exprnode("propertyNameLine", propertyNameLine)
				.Exprnode("propertyNameFile", propertyNameFile)
				.Exprnode("dateFormat", dateFormat)
				.Map("adapterInputSource", adapterInputSource)
				.Eventtype("outputEventType", _outputEventType)
				.Eventtypes("outputPortTypes", _outputPortTypes)
				.Build();
		}
	}
} // end of namespace
