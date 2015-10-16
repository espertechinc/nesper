///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.view.ext;
using com.espertech.esper.view.std;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Utility methods to deal with chains of views, and for merge/group-by views.
    /// </summary>
    public class ViewServiceHelper
    {
        public static ICollection<String> GetUniqueCandidateProperties(IList<ViewFactory> viewFactory, Attribute[] annotations)
        {
            var disableUniqueImplicit = HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX.GetHint(annotations) != null;
            if (viewFactory == null || viewFactory.IsEmpty())
            {
                return null;
            }
            if (viewFactory[0] is GroupByViewFactoryMarker)
            {
                var criteria = ((GroupByViewFactoryMarker) viewFactory[0]).CriteriaExpressions;
                var groupedCriteria = ExprNodeUtility.GetPropertyNamesIfAllProps(criteria);
                if (groupedCriteria == null)
                {
                    return null;
                }
                if (viewFactory[1] is DataWindowViewFactoryUniqueCandidate && !disableUniqueImplicit)
                {
                    var uniqueFactory = (DataWindowViewFactoryUniqueCandidate) viewFactory[1];
                    var uniqueCandidates = uniqueFactory.UniquenessCandidatePropertyNames;
                    if (uniqueCandidates != null)
                    {
                        uniqueCandidates.AddAll(groupedCriteria);
                    }
                    return uniqueCandidates;
                }
                return null;
            }
            else if (viewFactory[0] is DataWindowViewFactoryUniqueCandidate && !disableUniqueImplicit)
            {
                var uniqueFactory = (DataWindowViewFactoryUniqueCandidate) viewFactory[0];
                return uniqueFactory.UniquenessCandidatePropertyNames;
            }
            else if (viewFactory[0] is VirtualDWViewFactory)
            {
                var vdw = (VirtualDWViewFactory) viewFactory[0];
                return vdw.UniqueKeys;
            }
            return null;
        }

        public static IStreamRandomAccess GetOptPreviousExprRandomAccess(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRandomAccess randomAccess = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                var getter = (RandomAccessByIndexGetter) agentInstanceViewFactoryContext.PreviousNodeGetter;
                randomAccess = new IStreamRandomAccess(getter);
                getter.Updated(randomAccess);
            }
            return randomAccess;
        }
    
        public static IStreamRelativeAccess GetOptPreviousExprRelativeAccess(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRelativeAccess relativeAccessByEvent = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                var getter = (RelativeAccessByEventNIndexMap) agentInstanceViewFactoryContext.PreviousNodeGetter;
                relativeAccessByEvent = new IStreamRelativeAccess(getter.Updated);
                getter.Updated(relativeAccessByEvent, null);
            }
    
            return relativeAccessByEvent;
        }
    
        public static IStreamSortRankRandomAccess GetOptPreviousExprSortedRankedAccess(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamSortRankRandomAccess rankedRandomAccess = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                RandomAccessByIndexGetter getter = (RandomAccessByIndexGetter) agentInstanceViewFactoryContext.PreviousNodeGetter;
                rankedRandomAccess = new IStreamSortRankRandomAccess(getter);
                getter.Updated(rankedRandomAccess);
            }
    
            return rankedRandomAccess;
        }
    
        /// <summary>Add merge views for any views in the chain requiring a merge (group view). Appends to the list of view specifications passed in one ore more new view specifications that represent merge views. Merge views have the same parameter list as the (group) view they merge data for. </summary>
        /// <param name="specifications">is a list of view definitions defining the chain of views.</param>
        /// <throws>ViewProcessingException indicating that the view chain configuration is invalid</throws>
        protected internal static void AddMergeViews(IList<ViewSpec> specifications)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".addMergeViews Incoming specifications=" + specifications.Render());
            }
    
            // A grouping view requires a merge view and cannot be last since it would not group sub-views
            if (specifications.Count > 0)
            {
                var lastView = specifications[specifications.Count - 1];
                var viewEnum = ViewEnumExtensions.ForName(lastView.ObjectNamespace, lastView.ObjectName);
                if ((viewEnum != null) && (viewEnum.Value.GetMergeView() != null))
                {
                    throw new ViewProcessingException("Invalid use of the '" + lastView.ObjectNamespace + ":" +
                                lastView.ObjectName + "' view, the view requires one or more child views to group, or consider using the group-by clause");
                }
            }
    
            var mergeViewSpecs = new LinkedList<ViewSpec>();
    
            foreach (ViewSpec spec in specifications)
            {
                var viewEnum = ViewEnumExtensions.ForName(spec.ObjectNamespace, spec.ObjectName);
                if (viewEnum == null)
                {
                    continue;
                }
    
                if (viewEnum.Value.GetMergeView() == null)
                {
                    continue;
                }
    
                // The merge view gets the same parameters as the view that requires the merge
                ViewSpec mergeViewSpec = new ViewSpec(
                    viewEnum.Value.GetMergeView().Value.GetNamespace(),
                    viewEnum.Value.GetMergeView().Value.GetName(),
                    spec.ObjectParameters);
    
                // The merge views are added to the beginning of the list.
                // This enables group views to stagger ie. marketdata.Group("symbol").Group("feed").xxx.Merge(...).Merge(...)
                mergeViewSpecs.AddFirst(mergeViewSpec);
            }
    
            specifications.AddAll(mergeViewSpecs);
    
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".addMergeViews Outgoing specifications=" + specifications.Render());
            }
        }

        /// <summary>
        /// Instantiate a chain of views.
        /// </summary>
        /// <param name="parentViewable">parent view to add the chain to</param>
        /// <param name="viewFactories">is the view factories to use to make each view, or reuse and existing view</param>
        /// <param name="viewFactoryChainContext">The view factory chain context.</param>
        /// <returns>chain of views instantiated</returns>
        public static IList<View> InstantiateChain(Viewable parentViewable, IList<ViewFactory> viewFactories, AgentInstanceViewFactoryChainContext viewFactoryChainContext)
        {
            IList<View> newViews = new List<View>();
            Viewable parent = parentViewable;
    
            for (int i = 0; i < viewFactories.Count; i++) {
                ViewFactory viewFactory = viewFactories[i];
    
                // Create the new view object
                View currentView = viewFactory.MakeView(viewFactoryChainContext);
    
                newViews.Add(currentView);
                parent.AddView(currentView);
    
                // Next parent is the new view
                parent = currentView;
            }
    
            return newViews;
        }

        public static void RemoveFirstUnsharedView(IList<View> childViews)
        {
            for (int ii = childViews.Count - 1; ii >= 0; ii--)
            {
                var child = childViews[ii];
                var parent = child.Parent;
                if (parent == null)
                {
                    return;
                }
                parent.RemoveView(child);
                if (parent.HasViews)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Removes a view from a parent view returning the orphaned parent views in a list.
        /// </summary>
        /// <param name="parentViewable">parent to remove view from</param>
        /// <param name="viewToRemove">view to remove</param>
        /// <returns>
        /// chain of orphaned views
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="IllegalStateException">
        /// </exception>
        protected internal static IList<View> RemoveChainLeafView(Viewable parentViewable, Viewable viewToRemove)
        {
            IList<View> removedViews = new List<View>();
    
            // The view to remove must be a leaf node - non-leaf views are just not removed
            if (viewToRemove.HasViews)
            {
                return removedViews;
            }
    
            // Find child viewToRemove among descendent views
            IList<View> viewPath = ViewSupport.FindDescendent(parentViewable, viewToRemove);
    
            if (viewPath == null)
            {
                String message = "Viewable not found when removing view " + viewToRemove;
                throw new ArgumentException(message);
            }
    
            // The viewToRemove is a direct child view of the stream
            if (viewPath.IsEmpty())
            {
                bool isViewRemoved = parentViewable.RemoveView( (View) viewToRemove);
    
                if (!isViewRemoved)
                {
                    String message = "Failed to remove immediate child view " + viewToRemove;
                    Log.Fatal(".remove " + message);
                    throw new IllegalStateException(message);
                }
    
                removedViews.Add((View) viewToRemove);
                return removedViews;
            }
    
            View[] viewPathArray = viewPath.ToArray();
            View currentView = (View) viewToRemove;
    
            // Remove child from parent views until a parent view has more children,
            // or there are no more parents (index=0).
            for (int index = viewPathArray.Length - 1; index >= 0; index--)
            {
                bool isViewRemoved = viewPathArray[index].RemoveView(currentView);
                removedViews.Add(currentView);
    
                if (!isViewRemoved)
                {
                    String message = "Failed to remove view " + currentView;
                    Log.Fatal(".remove " + message);
                    throw new IllegalStateException(message);
                }
    
                // If the parent views has more child views, we are done
                if (viewPathArray[index].HasViews)
                {
                    break;
                }
    
                // The parent of the top parent is the stream, remove from stream
                if (index == 0)
                {
                    parentViewable.RemoveView(viewPathArray[0]);
                    removedViews.Add(viewPathArray[0]);
                }
                else
                {
                    currentView = viewPathArray[index];
                }
            }
    
            return removedViews;
        }

        /// <summary>
        /// Match the views under the stream to the list of view specications passed in. The method changes the view 
        /// specifications list passed in and removes those specifications for which matcing views have been found. 
        /// If none of the views under the stream matches the first view specification passed in, the method returns 
        /// the stream itself and leaves the view specification list unchanged. If one view under the stream matches, 
        /// the view's specification is removed from the list. The method will then attempt to determine if any child 
        /// views of that view also match specifications.
        /// </summary>
        /// <param name="rootViewable">is the top rootViewable event stream to which all views are attached as child views. This parameter is changed by this method, ie. specifications are removed if they match existing views.</param>
        /// <param name="viewFactories">is the view specifications for making views</param>
        /// <returns>
        /// a pair of (A) the stream if no views matched, or the last child view that matched (B) the full listof parent views
        /// </returns>
        protected internal static Pair<Viewable, IList<View>> MatchExistingViews(Viewable rootViewable, IList<ViewFactory> viewFactories)
        {
            Viewable currentParent = rootViewable;
            IList<View> matchedViewList = new List<View>();
    
            bool foundMatch;
    
            if (viewFactories.IsEmpty())
            {
                return new Pair<Viewable, IList<View>>(rootViewable, Collections.GetEmptyList<View>());
            }
    
            do      // while ((foundMatch) && (specifications.Count > 0));
            {
                foundMatch = false;
    
                foreach (View childView in currentParent.Views)
                {
                    ViewFactory currentFactory = viewFactories[0];
    
                    if (!(currentFactory.CanReuse(childView)))
                    {
                         continue;
                    }
    
                    // The specifications match, check current data window size
                    viewFactories.RemoveAt(0);
                    currentParent = childView;
                    foundMatch = true;
                    matchedViewList.Add(childView);
                    break;
                }
            }
            while ((foundMatch) && (viewFactories.IsNotEmpty()));
    
            return new Pair<Viewable, IList<View>>(currentParent, matchedViewList);
        }

        /// <summary>
        /// Given a list of view specifications obtained from by parsing this method instantiates a list of view
        /// factories. The view factories are not yet aware of each other after leaving this method (so not yet
        /// chained logically). They are simply instantiated and assigned view parameters.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="viewSpecList">is the view definition</param>
        /// <param name="statementContext">is statement service context and statement info</param>
        /// <returns>list of view factories</returns>
        /// <throws>ViewProcessingException if the factory cannot be creates such as for invalid view spec</throws>
        public static IList<ViewFactory> InstantiateFactories(int streamNum, IList<ViewSpec> viewSpecList, StatementContext statementContext)
        {
            IList<ViewFactory> factoryChain = new List<ViewFactory>();
    
            int viewNum = 0;
            foreach (ViewSpec spec in viewSpecList)
            {
                // Create the new view factory
                var viewFactory = statementContext.ViewResolutionService.Create(spec.ObjectNamespace, spec.ObjectName);
    
                var audit = AuditEnum.VIEW.GetAudit(statementContext.Annotations);
                if (audit != null) {
                    viewFactory = (ViewFactory) ViewFactoryProxy.NewInstance(statementContext.EngineURI, statementContext.StatementName, viewFactory, spec.ObjectName);
                }
                factoryChain.Add(viewFactory);

                // Set view factory parameters
                try
                {
                    ViewFactoryContext context = new ViewFactoryContext(statementContext, streamNum, viewNum, spec.ObjectNamespace, spec.ObjectName);
                    viewFactory.SetViewParameters(context, spec.ObjectParameters);
                }
                catch (ViewParameterException e)
                {
                    throw new ViewProcessingException(string.Format("Error in view '{0}{1}{2}', {3}", spec.ObjectNamespace, ':', spec.ObjectName, e.Message), e);
                }
                viewNum++;
            }
    
            return factoryChain;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
