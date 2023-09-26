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
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.render;
using com.espertech.esper.common.@internal.@event.util;

namespace com.espertech.esperio.file
{
	public class FileSinkCSV : DataFlowOperatorLifecycle, EPDataFlowSignalHandler {
	    private static readonly string NEWLINE = Environment.NewLine;

	    private readonly FileSinkFactory _factory;
	    private readonly string _filename;
	    private readonly bool _append;

	    private TextWriter _writer;
	    private RendererMeta _rendererMeta;
	    private RendererMetaOptions _rendererOptions;
	    private EventBeanSPI _eventShell;

	    public FileSinkCSV(FileSinkFactory factory, string filename, bool append) {
	        _factory = factory;
	        _filename = filename;
	        _append = append;

	        _rendererOptions = new RendererMetaOptions(true, false, null, null);
	        _rendererMeta = new RendererMeta(factory.EventType, new Stack<EventTypePropertyPair>(), _rendererOptions);
	        _eventShell = EventTypeUtility.GetShellForType(factory.EventType);
	    }

	    public void Open(DataFlowOpOpenContext openContext) {
	        var file = new FileInfo(_filename);
	        if (file.Exists && !_append) {
	            throw new EPException("File already exists '" + _filename + "'");
	        }

	        try {
		        if (_append) {
			        _writer = File.AppendText(file.ToString());
		        }
		        else {
			        _writer = File.CreateText(file.ToString());
		        }
	        } catch (FileNotFoundException e) {
	            throw new EPException($"Failed to open '{file}' for writing", e);
	        }
	    }

	    public void OnInput(object @object) {
	        try {
	            var buf = new StringBuilder();
	            if (!(_eventShell.EventType is JsonEventType)) {
		            _eventShell.Underlying = @object;
	            }
	            else {
		            var jsonEventType = (JsonEventType) _eventShell.EventType;
		            var underlying = jsonEventType.Parse(@object.ToString());
		            _eventShell.Underlying = underlying;
	            }

	            RecursiveRender(_eventShell, buf, 0, _rendererMeta, _rendererOptions);
	            _writer.Write(buf.ToString());
	            _writer.Flush();
	        } catch (IOException) {
	            if (_writer != null) {
	                try {
	                    _writer.Close();
	                } catch (IOException) {
	                }
	                _writer = null;
	            }
	        }
	    }

	    public void OnSignal(EPDataFlowSignal signal) {
	        if (signal is EPDataFlowSignalFinalMarker) {
	            Destroy();
	        }
	    }

	    public void Close(DataFlowOpCloseContext openContext) {
	        Destroy();
	    }

	    private static void RecursiveRender(
		    EventBean theEvent,
		    StringBuilder buf,
		    int level,
		    RendererMeta meta,
		    RendererMetaOptions rendererOptions)
	    {
		    var delimiter = "";
		    var simpleProps = meta.SimpleProperties;
		    foreach (var simpleProp in simpleProps) {
			    var value = simpleProp.Getter.Get(theEvent);
			    buf.Append(delimiter);
			    simpleProp.Output.Render(value, buf);
			    delimiter = ",";
		    }

		    buf.Append(NEWLINE);
	    }

	    private void Destroy() {
	        if (_writer != null) {
	            try {
	                _writer.Close();
	            } catch (IOException) {
	            }
	            _writer = null;
	        }
	    }
	}
} // end of namespace
