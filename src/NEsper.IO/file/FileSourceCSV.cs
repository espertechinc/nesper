///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esperio.csv;

namespace com.espertech.esperio.file
{
	public class FileSourceCSV : DataFlowSourceOperator
	{
		private readonly FileSourceFactory _factory;
		private readonly AdapterInputSource _adapterInputSource;
		private readonly bool _hasHeaderLine;
		private readonly bool _hasTitleLine;
		private readonly int? _numLoops;
		private readonly string _dateFormat;

		private StatementContext _statementContext;
		private ParseMakePropertiesDesc _parseMake;

		private bool _firstRow = true;
		private int _loopCount;

		[DataFlowContext] internal EPDataFlowEmitter graphContext;

		private CSVReader _reader;

		public FileSourceCSV(
			FileSourceFactory factory,
			DataFlowOpInitializeContext context,
			AdapterInputSource adapterInputSource,
			bool hasHeaderLine,
			bool hasTitleLine,
			int? numLoops,
			string[] propertyNames,
			string dateFormat)
		{
			_factory = factory;
			_adapterInputSource = adapterInputSource;
			_hasHeaderLine = hasHeaderLine;
			_hasTitleLine = hasTitleLine;
			_numLoops = numLoops;
			_dateFormat = dateFormat;

			_statementContext = context.AgentInstanceContext.StatementContext;

			// use event type's full list of properties
			if (!hasTitleLine) {
				if (propertyNames != null) {
					_parseMake = SetupProperties(false, propertyNames, factory.OutputEventType, _statementContext, dateFormat);
				}
				else {
					_parseMake = SetupProperties(false, factory.OutputEventType.PropertyNames, factory.OutputEventType, _statementContext, dateFormat);
				}
			}
		}

		public void Open(DataFlowOpOpenContext openContext)
		{
			_reader = new CSVReader(_adapterInputSource);
		}

		public void Next()
		{
			try {
				string[] nextRecord = _reader.GetNextRecord();

				if (_firstRow) {
					// determine the parsers from the title line
					if (_hasTitleLine && _parseMake == null) {
						_parseMake = SetupProperties(true, nextRecord, _factory.OutputEventType, _statementContext, _dateFormat);
					}

					if (_hasTitleLine || _hasHeaderLine) {
						nextRecord = _reader.GetNextRecord();
					}
				}

				var propertyIndexes = _parseMake.Indexes;
				var tuple = new object[propertyIndexes.Length];
				for (var i = 0; i < propertyIndexes.Length; i++) {
					tuple[i] = _parseMake.Parsers[i].Parse(nextRecord[propertyIndexes[i]]);
				}

				var underlying = _parseMake.EventBeanManufacturer.MakeUnderlying(tuple);

				if (underlying is object[]) {
					graphContext.Submit((object[]) underlying);
				}
				else {
					graphContext.Submit(underlying);
				}

				_firstRow = false;
			}
			catch (EndOfStreamException) {
				if (_numLoops != null) {
					_loopCount++;
					if (_loopCount >= _numLoops) {
						graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
					}
					else {
						// reset
						graphContext.SubmitSignal(new EPDataFlowSignalWindowMarkerImpl());
						_firstRow = true;
						if (_reader.IsResettable) {
							_reader.Reset();
						}
						else {
							_reader = new CSVReader(_adapterInputSource);
						}
					}
				}
				else {
					graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
				}
			}
		}

		public void Close(DataFlowOpCloseContext openContext)
		{
			if (_reader != null) {
				_reader.Close();
				_reader = null;
			}
		}

		private static ParseMakePropertiesDesc SetupProperties(bool requireOneMatch,
			string[] propertyNamesOffered,
			EventType outputEventType,
			StatementContext statementContext,
			string dateFormat)
		{
			var writeables = EventTypeUtility.GetWriteableProperties(outputEventType, false, false);

			IList<int> indexesList = new List<int>();
			IList<SimpleTypeParser> parserList = new List<SimpleTypeParser>();
			IList<WriteablePropertyDescriptor> writablesList = new List<WriteablePropertyDescriptor>();

			for (var i = 0; i < propertyNamesOffered.Length; i++) {
				var propertyName = propertyNamesOffered[i];
				Type propertyType;
				try {
					propertyType = outputEventType.GetPropertyType(propertyName);
				}
				catch (PropertyAccessException ex) {
					throw new EPException("Invalid property name '" + propertyName + "': " + ex.Message, ex);
				}

				if (propertyType == null) {
					continue;
				}

				SimpleTypeParser parser;
				if (propertyType.IsDateTime() && !propertyType.IsInt64()) {
					var dateTimeFormat = dateFormat != null
						? DateTimeFormat.For(dateFormat)
						: DateTimeFormat.ISO_DATE_TIME;

					if (propertyType == typeof(DateTime?)) {
						parser = new ProxySimpleTypeParser(
							text => (dateTimeFormat.Parse(text)?.DateTime)?.DateTime);
					} else if (propertyType == typeof(DateTime)) {
						parser = new ProxySimpleTypeParser(
							text => dateTimeFormat.Parse(text).DateTime.DateTime);
					}
					else if (propertyType == typeof(DateTimeOffset?)) {
						parser = new ProxySimpleTypeParser(
							text => dateTimeFormat.Parse(text)?.DateTime);
					}
					else if (propertyType == typeof(DateTimeOffset)) {
						parser = new ProxySimpleTypeParser(
							text => dateTimeFormat.Parse(text).DateTime);
					}
					else {
						parser = new ProxySimpleTypeParser(
							text => dateTimeFormat.Parse(text));
					}
				}
				else {
					parser = SimpleTypeParserFactory.GetParser(propertyType);
				}

				var writable = EventTypeUtility.FindWritable(propertyName, writeables);
				if (writable == null) {
					continue;
				}

				indexesList.Add(i);
				parserList.Add(parser);
				writablesList.Add(writable);
			}

			if (indexesList.IsEmpty() && requireOneMatch) {
				throw new EPException(
					"Failed to match any of the properties " +
					CompatExtensions.RenderAny(propertyNamesOffered) +
					" to the event type properties of event type '" +
					outputEventType.Name +
					"'");
			}

			var parsers = parserList.ToArray();
			var writables = writablesList.ToArray();
			var indexes = CollectionUtil.IntArray(indexesList);
			EventBeanManufacturer manufacturer;
			try {
				manufacturer = EventTypeUtility.GetManufacturer(
						outputEventType,
						writables,
						statementContext.ImportServiceRuntime,
						false,
						statementContext.EventTypeAvroHandler)
					.GetManufacturer(statementContext.EventBeanTypedEventFactory);
			}
			catch (EventBeanManufactureException e) {
				throw new EPException("Event type '" + outputEventType.Name + "' cannot be written to: " + e.Message, e);
			}

			return new ParseMakePropertiesDesc(indexes, parsers, manufacturer);
		}

		private class ParseMakePropertiesDesc
		{
			private readonly int[] _indexes;
			private readonly SimpleTypeParser[] _parsers;
			private readonly EventBeanManufacturer _eventBeanManufacturer;

			internal ParseMakePropertiesDesc(
				int[] indexes,
				SimpleTypeParser[] parsers,
				EventBeanManufacturer eventBeanManufacturer)
			{
				_indexes = indexes;
				_parsers = parsers;
				_eventBeanManufacturer = eventBeanManufacturer;
			}

			public int[] Indexes => _indexes;

			public SimpleTypeParser[] Parsers => _parsers;

			public EventBeanManufacturer EventBeanManufacturer => _eventBeanManufacturer;
		}
	}
} // end of namespace
