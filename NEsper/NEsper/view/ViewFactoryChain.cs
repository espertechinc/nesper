///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view
{
	/// <summary>
	/// Holder for the logical chain of view factories.
	/// </summary>
	public class ViewFactoryChain
	{
	    private readonly IList<ViewFactory> _viewFactoryChain;
	    private readonly EventType _streamEventType;

	    /// <summary>Ctor.</summary>
	    /// <param name="streamEventType">is the event type of the event stream</param>
	    /// <param name="viewFactoryChain">is the chain of view factories</param>
	    public ViewFactoryChain(EventType streamEventType, IList<ViewFactory> viewFactoryChain)
	    {
	        _streamEventType = streamEventType;
	        _viewFactoryChain = viewFactoryChain;
	    }

	    /// <summary>
	    /// Returns the final event type which is the event type of the last view factory in the chain,
	    /// or if the chain is empty then the stream's event type.
	    /// </summary>
	    /// <returns>final event type of the last view or stream</returns>
	    public EventType EventType
	    {
	    	get
	    	{
		        if (_viewFactoryChain.Count == 0)
		        {
		            return _streamEventType;
		        }
		        else
		        {
		        	return _viewFactoryChain[_viewFactoryChain.Count - 1].EventType;
		        }
	        }
	    }

	    /// <summary>Returns the chain of view factories.</summary>
	    /// <returns>view factory list</returns>
	    public IList<ViewFactory> FactoryChain
	    {
	    	get { return _viewFactoryChain; }
	    }


	    /// <summary>
	    /// Returns the number of data window factories for the chain.
	    /// </summary>
	    /// <returns>
	    /// number of data window factories
	    /// </returns>
	    public int DataWindowViewFactoryCount
	    {
	        get
	        {
	            int count = 0;
	            foreach (var chainElement in _viewFactoryChain) {
	                if (chainElement is DataWindowViewFactory) {
	                    count++;
	                }
	            }
	            return count;
	        }
	    }

        public static ViewFactoryChain FromTypeNoViews(EventType eventType)
        {
            return new ViewFactoryChain(eventType, Collections.GetEmptyList<ViewFactory>());
        }
	}
} // End of namespace
