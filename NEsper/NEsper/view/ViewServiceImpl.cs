///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.view.internals;
using com.espertech.esper.view.std;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Implementation of the view evaluation service business interface.
    /// </summary>
    public sealed class ViewServiceImpl : ViewService
    {
        /// <summary>Ctor. </summary>
        public ViewServiceImpl()
        {
        }

        public ViewFactoryChain CreateFactories(int streamNum, EventType parentEventType, ViewSpec[] viewSpecDefinitions, StreamSpecOptions options, StatementContext context, bool isSubquery, int subqueryNumber)
        {
            // Clone the view spec list to prevent parameter modification
            IList<ViewSpec> viewSpecList = new List<ViewSpec>(viewSpecDefinitions);

            // Inspect views and add merge views if required
            ViewServiceHelper.AddMergeViews(viewSpecList);

            // Instantiate factories, not making them aware of each other yet
            var viewFactories = ViewServiceHelper.InstantiateFactories(streamNum, viewSpecList, context, isSubquery, subqueryNumber);

            ViewFactory parentViewFactory = null;
            IList<ViewFactory> attachedViewFactories = new List<ViewFactory>();
            for (var i = 0; i < viewFactories.Count; i++)
            {
                var factoryToAttach = viewFactories[i];
                try
                {
                    factoryToAttach.Attach(parentEventType, context, parentViewFactory, attachedViewFactories);
                    attachedViewFactories.Add(viewFactories[i]);
                    parentEventType = factoryToAttach.EventType;
                }
                catch (ViewParameterException ex)
                {
                    var text = "Error attaching view to parent view";
                    if (i == 0)
                    {
                        text = "Error attaching view to event stream";
                    }
                    throw new ViewProcessingException(text + ": " + ex.Message, ex);
                }
            }

            // obtain count of data windows
            var dataWindowCount = 0;
            var firstNonDataWindowIndex = -1;
            for (var i = 0; i < viewFactories.Count; i++)
            {
                var factory = viewFactories[i];
                if (factory is DataWindowViewFactory)
                {
                    dataWindowCount++;
                    continue;
                }
                if ((factory is GroupByViewFactoryMarker) || (factory is MergeViewFactory))
                {
                    continue;
                }
                if (firstNonDataWindowIndex == -1)
                {
                    firstNonDataWindowIndex = i;
                }
            }

            var isAllowMultipleExpiry = context.ConfigSnapshot.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies;
            var isRetainIntersection = options.IsRetainIntersection;
            var isRetainUnion = options.IsRetainUnion;

            // Set the default to retain-intersection unless allow-multiple-expiry is turned on
            if ((!isAllowMultipleExpiry) && (!isRetainUnion))
            {
                isRetainIntersection = true;
            }

            // handle multiple data windows with retain union.
            // wrap view factories into the union view factory and handle a group-by, if present
            if ((isRetainUnion || isRetainIntersection) && dataWindowCount > 1)
            {
                viewFactories = GetRetainViewFactories(parentEventType, viewFactories, isRetainUnion, context);
            }

            return new ViewFactoryChain(parentEventType, viewFactories);
        }

        private IList<ViewFactory> GetRetainViewFactories(EventType parentEventType, IList<ViewFactory> viewFactories, bool isUnion, StatementContext context)
        {
            ICollection<int> groupByFactory = new HashSet<int>();
            ICollection<int> mergeFactory = new HashSet<int>();
            IList<ViewFactory> derivedValueViews = new List<ViewFactory>();
            IList<ViewFactory> dataWindowViews = new List<ViewFactory>();
            for (var i = 0; i < viewFactories.Count; i++)
            {
                var factory = viewFactories[i];
                if (factory is GroupByViewFactoryMarker)
                {
                    groupByFactory.Add(i);
                }
                else if (factory is MergeViewFactoryMarker)
                {
                    mergeFactory.Add(i);
                }
                else if (factory is DataWindowViewFactory)
                {
                    dataWindowViews.Add(factory);
                }
                else
                {
                    derivedValueViews.Add(factory);
                }
            }

            if (groupByFactory.Count > 1)
            {
                throw new ViewProcessingException("Multiple groupwin views are not allowed in conjuntion with multiple data windows");
            }
            if ((groupByFactory.IsNotEmpty()) && (groupByFactory.First() != 0))
            {
                throw new ViewProcessingException("The groupwin view must occur in the first position in conjuntion with multiple data windows");
            }
            if ((groupByFactory.IsNotEmpty()) && (mergeFactory.First() != (viewFactories.Count - 1)))
            {
                throw new ViewProcessingException("The merge view cannot be used in conjuntion with multiple data windows");
            }

            GroupByViewFactoryMarker groupByViewFactory = null;
            MergeViewFactoryMarker mergeViewFactory = null;
            if (groupByFactory.IsNotEmpty())
            {
                groupByViewFactory = (GroupByViewFactoryMarker)viewFactories[0];
                mergeViewFactory = (MergeViewFactoryMarker)viewFactories[viewFactories.Count - 1];
                viewFactories.RemoveAt(0);
                viewFactories.RemoveAt(viewFactories.Count - 1);

            }

            var container = context.Container;

            ViewFactory retainPolicy;
            if (isUnion)
            {
                var viewFactory = (UnionViewFactory)context.ViewResolutionService.Create(container, "internal", "union");
                viewFactory.ParentEventType = parentEventType;
                viewFactory.ViewFactories = dataWindowViews;
                retainPolicy = viewFactory;
            }
            else
            {
                var viewFactory = (IntersectViewFactory)context.ViewResolutionService.Create(container, "internal", "intersect");
                viewFactory.ParentEventType = parentEventType;
                viewFactory.ViewFactories = dataWindowViews;
                retainPolicy = viewFactory;
            }

            IList<ViewFactory> nonRetainViewFactories = new List<ViewFactory>();
            nonRetainViewFactories.Add(retainPolicy);
            if (groupByViewFactory != null)
            {
                nonRetainViewFactories.Insert(0, (ViewFactory)groupByViewFactory);
                nonRetainViewFactories.AddAll(derivedValueViews);
                nonRetainViewFactories.Add((ViewFactory)mergeViewFactory);
            }
            else
            {
                nonRetainViewFactories.AddAll(derivedValueViews);
            }

            return nonRetainViewFactories;
        }

        public ViewServiceCreateResult CreateViews(
            Viewable eventStreamViewable,
            IList<ViewFactory> viewFactories,
            AgentInstanceViewFactoryChainContext viewFactoryChainContext,
            bool hasPreviousNode)
        {
            // Attempt to find existing views under the stream that match specs.
            // The viewSpecList may have been changed by this method.
            Pair<Viewable, IList<View>> resultPair;
            if (hasPreviousNode)
            {
                resultPair = new Pair<Viewable, IList<View>>(eventStreamViewable, Collections.GetEmptyList<View>());
            }
            else
            {
                resultPair = ViewServiceHelper.MatchExistingViews(eventStreamViewable, viewFactories, viewFactoryChainContext.AgentInstanceContext);
            }

            var parentViewable = resultPair.First;

            if (viewFactories.IsEmpty())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".createView No new views created, dumping stream ... " + eventStreamViewable);
                    ViewSupport.DumpChildViews("EventStream ", eventStreamViewable);
                }

                return new ViewServiceCreateResult(parentViewable, parentViewable, Collections.GetEmptyList<View>());   // we know its a view here since the factory list is empty
            }

            // Instantiate remaining chain of views from the remaining factories which didn't match to existing views.
            var views = ViewServiceHelper.InstantiateChain(parentViewable, viewFactories, viewFactoryChainContext);

            // Initialize any views that need initializing after the chain is complete
            foreach (var view in views)
            {
                if (view is InitializableView)
                {
                    var initView = (InitializableView)view;
                    initView.Initialize();
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".createView New views created for stream, all views ... " + eventStreamViewable);
                ViewSupport.DumpChildViews("EventStream ", eventStreamViewable);
            }

            return new ViewServiceCreateResult(views[views.Count - 1], views[0], views);
        }

        public void Remove(EventStream eventStream, Viewable viewToRemove)
        {
            // If the viewToRemove to remove has child viewToRemove, don't disconnect - the child ViewToRemove(s) need this
            if (viewToRemove.HasViews)
            {
                return;
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".remove Views before the remove of view " + viewToRemove + ", for event stream " + eventStream);
                ViewSupport.DumpChildViews("EventStream ", eventStream);
            }

            // Remove views in chain leaving only non-empty parent views to the child view to be removed
            ViewServiceHelper.RemoveChainLeafView(eventStream, viewToRemove);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".remove Views after the remove, for event stream " + eventStream);
                ViewSupport.DumpChildViews("EventStream ", eventStream);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
