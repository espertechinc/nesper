///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    /// <summary>
    /// A helper class for View implementations that provides generic implementation for some of the 
    /// methods. Methods that contain the actual logic of the view are not implemented in this class. 
    /// A common implementation normally does not need to override any of the methods implemented here, 
    /// their implementation is generic and should suffice. The class provides a convenience method for 
    /// updating it's children data UpdateChildren(Object[], Object[]). This method should be called 
    /// from within the View.Update(Object[], Object[]) methods in the subclasses.
    /// </summary>
    public abstract class ViewSupport : View
    {
        public readonly static View[] EMPTY_VIEW_ARRAY = new View[0];
    
        /// <summary>Parent viewable to this view - directly accessible by subclasses. </summary>
        private Viewable _parent;
    
        private View[] _children;
    
        /// <summary>Constructor. </summary>
        protected ViewSupport()
        {
            _children = EMPTY_VIEW_ARRAY;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
        public abstract EventType EventType { get; }
        public abstract void Update(EventBean[] newData, EventBean[] oldData);

        public virtual Viewable Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public virtual View AddView(View view)
        {
            _children = AddView(_children, view);
            view.Parent = this;
            return view;
        }
    
        public virtual bool RemoveView(View view)
        {
            int index = FindViewIndex(_children, view);
            if (index == -1) {
                return false;
            }
            _children = RemoveView(_children, index);
            view.Parent = null;
            return true;
        }

        public virtual void RemoveAllViews()
        {
            _children = EMPTY_VIEW_ARRAY;
        }

        public virtual View[] Views
        {
            get { return _children; }
        }

        public virtual bool HasViews
        {
            get { return _children.Length > 0; }
        }

        /// <summary>Updates all the children with new data. Views may want to use the hasViews method on the Viewable interface to determine if there are any child views attached at all, and save the work of constructing the arrays and making the call to UpdateChildren() in case there aren't any children attached. </summary>
        /// <param name="newData">is the array of new event data</param>
        /// <param name="oldData">is the array of old event data</param>
        public virtual void UpdateChildren(EventBean[] newData, EventBean[] oldData)
        {
            int size = _children.Length;
    
            // Provide a shortcut for a single child view since this is a very common case.
            // No iteration required here.
            if (size == 0)
            {
                return;
            }
            if (size == 1)
            {
                _children[0].Update(newData, oldData);
            }
            else
            {
                // since there often is zero or one view underneath, the iteration case is slower
                foreach (View child in _children)
                {
                    child.Update(newData, oldData);
                }
            }
        }
    
        /// <summary>Updates all the children with new data. Static convenience method that accepts the list of child views as a parameter. </summary>
        /// <param name="childViews">is the list of child views to send the data to</param>
        /// <param name="newData">is the array of new event data</param>
        /// <param name="oldData">is the array of old event data</param>
        internal static void UpdateChildren(ICollection<View> childViews, EventBean[] newData, EventBean[] oldData)
        {
            foreach (View child in childViews)
            {
                child.Update(newData, oldData);
            }
        }
    
        /// <summary>Convenience method for logging the parameters passed to the Update method. Only logs if debug is enabled. </summary>
        /// <param name="prefix">is a prefix text to output for each line</param>
        /// <param name="result">is the data in an Update call</param>
        public static void DumpUpdateParams(String prefix, UniformPair<EventBean[]> result)
        {
            EventBean[] newEventArr = result != null ? result.First : null;
            EventBean[] oldEventArr = result != null ? result.Second : null;
            DumpUpdateParams(prefix, newEventArr, oldEventArr);
        }
    
        /// <summary>Convenience method for logging the parameters passed to the Update method. Only logs if debug is enabled. </summary>
        /// <param name="prefix">is a prefix text to output for each line</param>
        /// <param name="newData">is the new data in an Update call</param>
        /// <param name="oldData">is the old data in an Update call</param>
        public static void DumpUpdateParams(String prefix, Object[] newData, Object[] oldData)
        {
            if (!Log.IsDebugEnabled)
            {
                return;
            }
    
            var writer = new StringWriter();
            if (newData == null)
            {
                writer.WriteLine(prefix + " newData=null ");
            }
            else
            {
                writer.WriteLine(prefix + " newData.size=" + newData.Length + "...");
                PrintObjectArray(prefix, writer, newData);
            }
    
            if (oldData == null)
            {
                writer.WriteLine(prefix + " oldData=null ");
            }
            else
            {
                writer.WriteLine(prefix + " oldData.size=" + oldData.Length + "...");
                PrintObjectArray(prefix, writer, oldData);
            }
        }
    
        private static void PrintObjectArray(String prefix, TextWriter writer, Object[] objects)
        {
            int count = 0;
            foreach (Object @object in objects)
            {
                var objectToString = (@object == null) ? "null" : @object.ToString();
                writer.WriteLine(prefix + " #" + count + " = " + objectToString);
            }
        }
    
        /// <summary>Convenience method for logging the child views of a Viewable. Only logs if debug is enabled. This is a recursive method. </summary>
        /// <param name="prefix">is a text to print for each view printed</param>
        /// <param name="parentViewable">is the parent for which the child views are displayed.</param>
        public static void DumpChildViews(String prefix, Viewable parentViewable)
        {
            if (Log.IsDebugEnabled)
            {
                if (parentViewable != null && parentViewable.Views != null) {
                    foreach (View child in parentViewable.Views)
                    {
                        if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                        {
                            Log.Debug(".dumpChildViews " + prefix + ' ' + child);
                        }
                        DumpChildViews(prefix + "  ", child);
                    }
                }
            }
        }

        /// <summary>
        /// Find the descendent view in the view tree under the parent view returning the list of view nodes between the parent view and the descendent view. Returns null if the descendent view is not found. Returns an empty list if the descendent view is a child view of the parent view.
        /// </summary>
        /// <param name="parentView">is the view to start searching under</param>
        /// <param name="descendentView">is the view to find</param>
        /// <returns>
        /// list of Viewable nodes between parent and descendent view.
        /// </returns>
        public static IList<View> FindDescendent(Viewable parentView, Viewable descendentView)
        {
            var stack = new Stack<View>();

            foreach (View view in parentView.Views)
            {
                if (view == descendentView)
                {
                    var viewList = new List<View>(stack);
                    viewList.Reverse();
                    return viewList;
                }
    
                bool found = FindDescendentRecusive(view, descendentView, stack);
    
                if (found)
                {
                    var viewList = new List<View>(stack);
                    viewList.Reverse();
                    return viewList;
                }
            }
    
            return null;
        }
    
        private static bool FindDescendentRecusive(View parentView, Viewable descendentView, Stack<View> stack)
        {
            stack.Push(parentView);
    
            bool found = false;
            foreach (View view in parentView.Views)
            {
                if (view == descendentView)
                {
                    return true;
                }
    
                found = FindDescendentRecusive(view, descendentView, stack);
    
                if (found)
                {
                    break;
                }
            }
    
            if (!found)
            {
                stack.Pop();
                return false;
            }
    
            return true;
        }
    
        public static View[] AddView(View[] children, View view)
        {
            if (children.Length == 0) {
                return new View[] {view};
            }
            else {
                return (View[]) (CollectionUtil.ArrayExpandAddSingle(children, view));
            }
        }
    
        public static int FindViewIndex(View[] children, View view) {
            for (int i = 0; i < children.Length; i++) {
                if (children[i] == view) {
                    return i;
                }
            }
            return -1;
        }
    
        public static View[] RemoveView(View[] children, int index) {
            if (children.Length == 1) {
                return EMPTY_VIEW_ARRAY;
            }
            else {
                return (View[]) (CollectionUtil.ArrayShrinkRemoveSingle(children, index));
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
