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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esperio.file
{
	public class FileSourceFactory : DataFlowOperatorFactory
	{
		public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
		{
		}

		public DataFlowOperator Operator(DataFlowOpInitializeContext context)
		{
			var container = context.Container;
			var adapterInputSourceValue = DataFlowParameterResolution.ResolveOptionalInstance<AdapterInputSource>(
				"adapterInputSource",
				AdapterInputSource,
				context);
			var fileName = DataFlowParameterResolution.ResolveWithDefault<string>(
				"file",
				File,
				null,
				context);

			if (adapterInputSourceValue == null) {
				if (fileName != null) {
					adapterInputSourceValue = new AdapterInputSource(container, new FileInfo(fileName));
				}
				else {
					throw new EPException("Failed to find required parameter, either the file or the adapterInputSource parameter is required");
				}
			}

			var formatValue = DataFlowParameterResolution.ResolveStringOptional("format", Format, context);
			switch (formatValue) {
				case null:
				case "csv": {
					var hasHeaderLineFlag = DataFlowParameterResolution.ResolveWithDefault<bool?>(
						"hasHeaderLine",
						HasHeaderLine,
						false,
						context);
					var hasTitleLineFlag = DataFlowParameterResolution.ResolveWithDefault<bool?>(
						"hasTitleLine",
						HasTitleLine,
						false,
						context);
					var numLoopsValue = DataFlowParameterResolution.ResolveWithDefault<int?>(
						"numLoops",
						NumLoops,
						null,
						context);
					var dateFormatValue = DataFlowParameterResolution.ResolveStringOptional(
						"dateFormat",
						DateFormat,
						context);

					return new FileSourceCSV(
						this,
						context,
						adapterInputSourceValue,
						hasHeaderLineFlag ?? false,
						hasTitleLineFlag ?? false,
						numLoopsValue,
						PropertyNames,
						dateFormatValue);
				}

				case "line": {
					var propertyNameLineValue = DataFlowParameterResolution.ResolveStringOptional("propertyNameLine", PropertyNameLine, context);
					var propertyNameFileValue = DataFlowParameterResolution.ResolveStringOptional("propertyNameFile", PropertyNameFile, context);
					return new FileSourceLineUnformatted(this, context, adapterInputSourceValue, fileName, propertyNameLineValue, propertyNameFileValue);
				}

				default:
					throw new ArgumentException("Unrecognized file format '" + formatValue + "'");
			}
		}

		public ExprEvaluator File { get; set; }

		public ExprEvaluator HasHeaderLine { get; set; }

		public ExprEvaluator HasTitleLine { get; set; }

		public IDictionary<string, object> AdapterInputSource { get; set; }

		public ExprEvaluator NumLoops { get; set; }

		public string[] PropertyNames { get; set; }

		public ExprEvaluator Format { get; set; }


		public ExprEvaluator PropertyNameLine { get; set; }

		public ExprEvaluator PropertyNameFile { get; set; }

		public ExprEvaluator DateFormat { get; set; }

		public EventType OutputEventType { get; set; }

		public EventType[] OutputPortTypes { get; set; }
	}
} // end of namespace
