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

		[DataFlowOpParameter] private ExprNode _file;

		[DataFlowOpParameter] private ExprNode _classpathFile;

		[DataFlowOpParameter] private ExprNode _hasHeaderLine;

		[DataFlowOpParameter] private ExprNode _hasTitleLine;

		[DataFlowOpParameter] private IDictionary<string, object> _adapterInputSource;

		[DataFlowOpParameter] private ExprNode _numLoops;

		[DataFlowOpParameter] private string[] _propertyNames;

		[DataFlowOpParameter] private ExprNode _format;

		[DataFlowOpParameter] private ExprNode _propertyNameLine;

		[DataFlowOpParameter] private ExprNode _propertyNameFile;

		[DataFlowOpParameter] private ExprNode _dateFormat;

		private EventType _outputEventType;

		private EventType[] _outputPortTypes;

		public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
		{
			_outputEventType = context.OutputPorts.Get(0).OptionalDeclaredType != null ? context.OutputPorts.Get(0).OptionalDeclaredType.EventType : null;
			if (_outputEventType == null) {
				throw new ExprValidationException("No event type provided for output, please provide an event type name");
			}

			_outputPortTypes = new EventType[context.OutputPorts.Count];
			foreach (var entry in context.OutputPorts) {
				_outputPortTypes[entry.Key] = entry.Value.OptionalDeclaredType.EventType;
			}

			_file = DataFlowParameterValidation.Validate("file", _file, typeof(string), context);
			_classpathFile = DataFlowParameterValidation.Validate("classpathFile", _classpathFile, typeof(bool), context);
			_hasHeaderLine = DataFlowParameterValidation.Validate("hasHeaderLine", _hasHeaderLine, typeof(bool), context);
			_hasTitleLine = DataFlowParameterValidation.Validate("hasTitleLine", _hasTitleLine, typeof(bool), context);
			_numLoops = DataFlowParameterValidation.Validate("numLoops", _numLoops, typeof(int?), context);
			_format = DataFlowParameterValidation.Validate("format", _format, typeof(string), context);
			_propertyNameLine = DataFlowParameterValidation.Validate("propertyNameLine", _propertyNameLine, typeof(string), context);
			_propertyNameFile = DataFlowParameterValidation.Validate("propertyNameFile", _propertyNameFile, typeof(string), context);
			_dateFormat = DataFlowParameterValidation.Validate("dateFormat", _dateFormat, typeof(string), context);
			return null;
		}

		public CodegenExpression Make(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			return new SAIFFInitializeBuilder(typeof(FileSourceFactory), GetType(), "factory", parent, symbols, classScope)
				.Exprnode("file", _file)
				.Exprnode("classpathFile", _classpathFile)
				.Exprnode("hasHeaderLine", _hasHeaderLine)
				.Exprnode("hasTitleLine", _hasTitleLine)
				.Exprnode("numLoops", _numLoops)
				.Constant("propertyNames", _propertyNames)
				.Exprnode("format", _format)
				.Exprnode("propertyNameLine", _propertyNameLine)
				.Exprnode("propertyNameFile", _propertyNameFile)
				.Exprnode("dateFormat", _dateFormat)
				.Map("adapterInputSource", _adapterInputSource)
				.Eventtype("outputEventType", _outputEventType)
				.Eventtypes("outputPortTypes", _outputPortTypes)
				.Build();
		}
	}
} // end of namespace
