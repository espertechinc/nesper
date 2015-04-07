///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view
{
    /// <summary>
    /// The Viewable interface marks an object as supporting zero, one or more View instances. 
    /// All implementing classes must call each view's 'Update' method when new data enters it. 
    /// Implementations must take care to synchronize methods of this interface with other methods 
    /// such that data flow is threadsafe. 
    /// </summary>
    public interface Viewable : EventCollection
    {
        /// <summary>Add a view to the viewable object. </summary>
        /// <param name="view">to add</param>
        /// <returns>view to add</returns>
        View AddView(View view);

        /// <summary>Returns all added views. </summary>
        /// <value>list of added views</value>
        View[] Views { get; }

        /// <summary>Remove a view. </summary>
        /// <param name="view">to remove</param>
        /// <returns>true to indicate that the view to be removed existed within this view, false if the view toremove could not be found </returns>
        bool RemoveView(View view);
    
        /// <summary>Remove all views. </summary>
        void RemoveAllViews();

        /// <summary>Test is there are any views to the Viewable. </summary>
        /// <value>true indicating there are child views, false indicating there are no child views</value>
        bool HasViews { get; }
    }
}
