///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esperio.file
{
	public class FileSourceLineUnformatted : DataFlowSourceOperator
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly FileSourceFactory _factory;
		private readonly AdapterInputSource _inputSource;
		private readonly string _propertyNameLine;
		private readonly string _propertyNameFile;

		private Stream _stream;
		private TextReader _reader;

		[DataFlowContext] public EPDataFlowEmitter graphContext;

		private LineProcessor _lineProcessor;
		private FileBeginEndProcessor _bofProcessor;
		private FileBeginEndProcessor _eofProcessor;
		private string _filenameOrUri;
		private bool _first = true;

		public FileSourceLineUnformatted(
			FileSourceFactory factory,
			DataFlowOpInitializeContext context,
			AdapterInputSource inputSource,
			string filenameOrUri,
			string propertyNameLine,
			string propertyNameFile)
		{
			_factory = factory;
			_inputSource = inputSource;
			_filenameOrUri = filenameOrUri;
			_propertyNameLine = propertyNameLine;
			_propertyNameFile = propertyNameFile;

			var outputEventType = factory.OutputEventType;
			var statementContext = context.AgentInstanceContext.StatementContext;

			if ((outputEventType.PropertyNames.Length != 1 || outputEventType.PropertyDescriptors[0].PropertyType != typeof(string)) &&
			    propertyNameLine == null) {
				throw new ArgumentException(
					"Expecting an output event type that has a single property that is of type string, or alternatively specify the 'propertyNameLine' parameter");
			}

			if (outputEventType is ObjectArrayEventType && outputEventType.PropertyDescriptors.Count == 1) {
				_lineProcessor = new LineProcessorObjectArray();
			}
			else {
				var propertyNameLineToUse = propertyNameLine;
				if (propertyNameLineToUse == null) {
					propertyNameLineToUse = outputEventType.PropertyDescriptors[0].PropertyName;
				}

				if (!outputEventType.IsProperty(propertyNameLineToUse)) {
					throw new EPException("Failed to find property name '" + propertyNameLineToUse + "' in type '" + outputEventType.Name + "'");
				}

				Type propertyType;
				try {
					propertyType = outputEventType.GetPropertyType(propertyNameLineToUse);
				}
				catch (PropertyAccessException ex) {
					throw new EPException("Invalid property name '" + propertyNameLineToUse + "': " + ex.Message, ex);
				}

				if (propertyType != typeof(string)) {
					throw new EPException("Invalid property type for property '" + propertyNameLineToUse + "', expected a property of type String");
				}

				var writeables = EventTypeUtility.GetWriteableProperties(outputEventType, false, false);
				IList<WriteablePropertyDescriptor> writeableList = new List<WriteablePropertyDescriptor>();

				var writeableLine = EventTypeUtility.FindWritable(propertyNameLineToUse, writeables);
				if (writeableLine == null) {
					throw new EPException("Failed to find writable property property '" + propertyNameLineToUse + "', is the property read-only?");
				}

				writeableList.Add(writeableLine);

				if (propertyNameFile != null) {
					var writeableFile = EventTypeUtility.FindWritable(propertyNameFile, writeables);
					if (writeableFile == null || writeableFile.PropertyType != typeof(string)) {
						throw new EPException("Failed to find writable String-type property '" + propertyNameFile + "', is the property read-only?");
					}

					writeableList.Add(writeableFile);
				}

				EventBeanManufacturer manufacturer;
				try {
					var writables = writeableList.ToArray();
					manufacturer = EventTypeUtility
						.GetManufacturer(outputEventType, writables, statementContext.ImportServiceRuntime, false, statementContext.EventTypeAvroHandler)
						.GetManufacturer(statementContext.EventBeanTypedEventFactory);
				}
				catch (EventBeanManufactureException e) {
					throw new EPException("Event type '" + outputEventType.Name + "' cannot be written to: " + e.Message, e);
				}

				_lineProcessor = new LineProcessorGeneralPurpose(manufacturer);
			}

			if (factory.OutputPortTypes.Length == 2) {
				_eofProcessor = GetBeginEndProcessor(context, 1);
			}
			else if (factory.OutputPortTypes.Length == 3) {
				_bofProcessor = GetBeginEndProcessor(context, 1);
				_eofProcessor = GetBeginEndProcessor(context, 2);
			}
			else if (factory.OutputPortTypes.Length > 3) {
				throw new EPException("Operator only allows up to 3 output ports");
			}
		}

		public void Open(DataFlowOpOpenContext openContext)
		{
			var reader = _inputSource.GetAsReader();
			if (reader != null) {
				_reader = reader;
			}
			else {
				_reader = new StreamReader(_inputSource.GetAsStream());
			}
		}

		public void Close(DataFlowOpCloseContext openContext)
		{
			try {
				_reader?.Close();
				_stream?.Close();
			}
			catch (IOException ex) {
				Log.Error("Failed to close file: " + ex.Message, ex);
			}
		}

		public void Next()
		{
			if (_first) {
				_first = false;
				if (_bofProcessor != null) {
					graphContext.SubmitPort(1, _bofProcessor.ProcessXOF(_filenameOrUri));
				}
			}

			string line;
			try {
				line = _reader.ReadLine();
			}
			catch (IOException e) {
				throw new EPException("Failed to read line: " + e.Message, e);
			}

			if (line != null) {
				if (Log.IsDebugEnabled) {
					Log.Debug("Submitting line '" + line + "'");
				}

				if (_eofProcessor != null) {
					graphContext.SubmitPort(0, _lineProcessor.Process(line, _filenameOrUri));
				}
				else {
					graphContext.Submit(_lineProcessor.Process(line, _filenameOrUri));
				}
			}
			else {
				if (Log.IsDebugEnabled) {
					Log.Debug("Submitting punctuation");
				}

				if (_eofProcessor != null) {
					var port = _bofProcessor != null ? 2 : 1;
					graphContext.SubmitPort(port, _eofProcessor.ProcessXOF(_filenameOrUri));
				}

				graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
			}
		}

		private FileBeginEndProcessor GetBeginEndProcessor(DataFlowOpInitializeContext context,
			int outputPort)
		{
			var portEventType = _factory.OutputPortTypes[outputPort];
			var writeables = EventTypeUtility.GetWriteableProperties(portEventType, false, false);
			var writeableList = new List<WriteablePropertyDescriptor>();
			EventBeanManufacturer manufacturer;
			if (_propertyNameFile != null) {
				var writeableFile = EventTypeUtility.FindWritable(_propertyNameFile, writeables);
				if (writeableFile == null || writeableFile.PropertyType != typeof(string)) {
					throw new EPException("Failed to find writable String-type property '" + _propertyNameFile + "', is the property read-only?");
				}

				writeableList.Add(writeableFile);
			}

			try {
				manufacturer = EventTypeUtility.GetManufacturer(
						portEventType,
						writeableList.ToArray(),
						context.AgentInstanceContext.ImportServiceRuntime,
						false,
						context.AgentInstanceContext.EventTypeAvroHandler)
					.GetManufacturer(context.AgentInstanceContext.EventBeanTypedEventFactory);
			}
			catch (EventBeanManufactureException e) {
				throw new EPException("Event type '" + portEventType.Name + "' cannot be written to: " + e.Message, e);
			}

			return new FileBeginEndProcessorGeneralPurpose(manufacturer);
		}

		private interface LineProcessor
		{
			object Process(string line, string filename);
		}

		private class LineProcessorObjectArray : LineProcessor
		{
			public object Process(string line,
				string filename)
			{
				return new object[] {line};
			}
		}

		private class LineProcessorGeneralPurpose : LineProcessor
		{
			private readonly EventBeanManufacturer _manufacturer;

			public LineProcessorGeneralPurpose(EventBeanManufacturer manufacturer)
			{
				_manufacturer = manufacturer;
			}

			public object Process(string line,
				string filename)
			{
				return _manufacturer.MakeUnderlying(new object[] {line, filename});
			}
		}

		private interface FileBeginEndProcessor
		{
			object ProcessXOF(string filename);
		}

		private class FileBeginEndProcessorGeneralPurpose : FileBeginEndProcessor
		{
			private readonly EventBeanManufacturer _manufacturer;

			public FileBeginEndProcessorGeneralPurpose(EventBeanManufacturer manufacturer)
			{
				_manufacturer = manufacturer;
			}

			public object ProcessXOF(string filename)
			{
				return _manufacturer.MakeUnderlying(new object[] {filename});
			}
		}
	}
} // end of namespace
