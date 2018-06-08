///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// The group view splits the data in a stream to multiple subviews, based on a key index. 
    /// The key is one or more fields in the stream. Any view that follows the GROUP view will 
    /// be executed separately on each subview, one per unique key. The view takes a single 
    /// parameter which is the field name returning the key value to group. This view can, for 
    /// example, be used to calculate the average price per symbol for a list of symbols. The 
    /// view treats its child views and their child views as prototypes. It dynamically instantiates 
    /// copies of each child view and their child views, and the child view's child views as so on. 
    /// When there are no more child views or the special merge view is encountered, it ends. The 
    /// view installs a special merge view unto each leaf child view that merges the value key that 
    /// was grouped by back into the stream using the group-by field name.
    /// </summary>
    public class GroupByViewImpl : ViewSupport, CloneableView, GroupByView
    {
        public readonly static String VIEWNAME = "Group-By";

        private readonly ExprNode[] _criteriaExpressions;
        private readonly ExprEvaluator[] _criteriaEvaluators;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        private readonly String[] _propertyNames;
        private readonly IDictionary<Object, Object> _subViewsPerKey = new Dictionary<Object, Object>();

        private readonly Dictionary<Object, Pair<Object, Object>> _groupedEvents = new Dictionary<Object, Pair<Object, Object>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="criteriaExpressions">is the fields from which to pull the values to group by</param>
        /// <param name="criteriaEvaluators">The criteria evaluators.</param>
        public GroupByViewImpl(AgentInstanceViewFactoryChainContext agentInstanceContext, ExprNode[] criteriaExpressions, ExprEvaluator[] criteriaEvaluators)
        {
            _agentInstanceContext = agentInstanceContext;
            _criteriaExpressions = criteriaExpressions;
            _criteriaEvaluators = criteriaEvaluators;

            _propertyNames = new String[criteriaExpressions.Length];
            for (var i = 0; i < criteriaExpressions.Length; i++)
            {
                _propertyNames[i] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(criteriaExpressions[i]);
            }
        }

        public View CloneView()
        {
            return new GroupByViewImpl(_agentInstanceContext, _criteriaExpressions, _criteriaEvaluators);
        }

        /// <summary>Returns the field name that provides the key valie by which to group by. </summary>
        /// <value>field name providing group-by key.</value>
        public ExprNode[] CriteriaExpressions
        {
            get { return _criteriaExpressions; }
        }

        public override EventType EventType
        {
            get
            {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, "Grouped", newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                // Algorithm for single new event
                if ((newData != null) && (oldData == null) && (newData.Length == 1))
                {
                    var theEvent = newData[0];
                    var newDataToPost = new EventBean[]
                    {
                        theEvent
                    };

                    var groupByValuesKey = GetGroupKey(theEvent);

                    // Get child views that belong to this group-by value combination
                    var subViews = _subViewsPerKey.Get(groupByValuesKey);

                    // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
                    if (subViews == null)
                    {
                        subViews = MakeSubViews(this, _propertyNames, groupByValuesKey, _agentInstanceContext);
                        _subViewsPerKey.Put(groupByValuesKey, subViews);
                    }

                    UpdateChildViews(subViews, newDataToPost, null);
                }
                else
                {
                    // Algorithm for dispatching multiple events
                    if (newData != null)
                    {
                        foreach (var newValue in newData)
                        {
                            HandleEvent(newValue, true);
                        }
                    }

                    if (oldData != null)
                    {
                        foreach (var oldValue in oldData)
                        {
                            HandleEvent(oldValue, false);
                        }
                    }

                    // Update child views
                    foreach (var entry in _groupedEvents)
                    {
                        var newEvents = ConvertToArray(entry.Value.First);
                        var oldEvents = ConvertToArray(entry.Value.Second);
                        UpdateChildViews(entry.Key, newEvents, oldEvents);
                    }

                    _groupedEvents.Clear();
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException("Cannot iterate over group view, this operation is not supported");
        }

        public override String ToString()
        {
            return GetType().FullName + " groupFieldNames=" + _criteriaExpressions.Render();
        }

        /// <summary>
        /// Instantiate subviews for the given group view and the given key value to group-by. Makes shallow 
        /// copies of each child view and its subviews up to the merge point. Sets up merge data views for 
        /// merging the group-by key value back in.
        /// </summary>
        /// <param name="groupView">is the parent view for which to copy subviews for</param>
        /// <param name="propertyNames">names of expressions or properties</param>
        /// <param name="groupByValues">is the key values to group-by</param>
        /// <param name="agentInstanceContext">is the view services that sub-views may need</param>
        /// <returns>
        /// a single view or a list of views that are copies of the original list, with copied children, withdata merge views added to the copied child leaf views.
        /// </returns>
        public static Object MakeSubViews(
            GroupByView groupView,
            String[] propertyNames,
            Object groupByValues,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            if (!groupView.HasViews)
            {
                const string message = "Unexpected empty list of child nodes for group view";
                Log.Error(".copySubViews " + message);
                throw new EPException(message);
            }

            Object subviewHolder;
            if (groupView.Views.Length == 1)
            {
                subviewHolder = CopyChildView(groupView, propertyNames, groupByValues, agentInstanceContext, groupView.Views[0]);
            }
            else
            {
                // For each child node
                var subViewList = new List<View>(4);
                subviewHolder = subViewList;
                foreach (var originalChildView in groupView.Views)
                {
                    var copyChildView = CopyChildView(groupView, propertyNames, groupByValues, agentInstanceContext, originalChildView);
                    subViewList.Add(copyChildView);
                }
            }

            return subviewHolder;
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(VIEWNAME, _subViewsPerKey.Count);
            foreach (var entry in _subViewsPerKey)
            {
                VisitView(viewDataVisitor, entry.Key, entry.Value);
            }
        }

        public static void VisitView(ViewDataVisitorContained viewDataVisitor, Object groupkey, Object subviewHolder)
        {
            if (subviewHolder == null)
            {
                return;
            }
            if (subviewHolder is View)
            {
                viewDataVisitor.VisitContained(groupkey, (View)subviewHolder);
                return;
            }
            if (subviewHolder is ICollection<View>)
            {
                var deque = (ICollection<View>)subviewHolder;
                foreach (var view in deque)
                {
                    viewDataVisitor.VisitContained(groupkey, view);
                    return;
                }
            }
        }

        public override bool RemoveView(View view)
        {
            if (!(view is GroupableView))
            {
                base.RemoveView(view);
            }
            var removed = base.RemoveView(view);
            if (!removed)
            {
                return false;
            }
            if (!HasViews)
            {
                _subViewsPerKey.Clear();
                return true;
            }
            var removedView = (GroupableView)view;
            Deque<Object> removedKeys = null;
            foreach (var entry in _subViewsPerKey)
            {
                var value = entry.Value;
                if (value is View)
                {
                    var subview = (GroupableView)value;
                    if (CompareViews(subview, removedView))
                    {
                        if (removedKeys == null)
                        {
                            removedKeys = new ArrayDeque<Object>();
                        }
                        removedKeys.Add(entry.Key);
                    }
                }
                else if (value is IList<View>)
                {
                    var subviews = (IList<View>)value;
                    for (var i = 0; i < subviews.Count; i++)
                    {
                        var subview = (GroupableView)subviews[i];
                        if (CompareViews(subview, removedView))
                        {
                            subviews.RemoveAt(i);
                            if (subviews.IsEmpty())
                            {
                                if (removedKeys == null)
                                {
                                    removedKeys = new ArrayDeque<Object>();
                                }
                                removedKeys.Add(entry.Key);
                            }
                            break;
                        }
                    }
                }
            }
            if (removedKeys != null)
            {
                foreach (var key in removedKeys)
                {
                    _subViewsPerKey.Remove(key);
                }
            }
            return true;
        }

        private bool CompareViews(GroupableView subview, GroupableView removed)
        {
            return subview.ViewFactory == removed.ViewFactory;
        }

        public static void UpdateChildViews(Object subViews, EventBean[] newData, EventBean[] oldData)
        {
            if (subViews is IList<View>)
            {
                var viewList = (IList<View>)subViews;
                UpdateChildren(viewList, newData, oldData);
            }
            else
            {
                ((View)subViews).Update(newData, oldData);
            }
        }

        private void HandleEvent(EventBean theEvent, bool isNew)
        {
            var groupByValuesKey = GetGroupKey(theEvent);

            // Get child views that belong to this group-by value combination
            var subViews = _subViewsPerKey.Get(groupByValuesKey);

            // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
            if (subViews == null)
            {
                subViews = MakeSubViews(this, _propertyNames, groupByValuesKey, _agentInstanceContext);
                _subViewsPerKey.Put(groupByValuesKey, subViews);
            }

            // Construct a pair of lists to hold the events for the grouped value if not already there
            var pair = _groupedEvents.Get(subViews);
            if (pair == null)
            {
                pair = new Pair<Object, Object>(null, null);
                _groupedEvents.Put(subViews, pair);
            }

            // Add event to a child view event list for later child Update that includes new and old events
            if (isNew)
            {
                pair.First = AddUpgradeToDequeIfPopulated(pair.First, theEvent);
            }
            else
            {
                pair.Second = AddUpgradeToDequeIfPopulated(pair.Second, theEvent);
            }
        }

        private static View CopyChildView(GroupByView groupView, String[] propertyNames, Object groupByValues, AgentInstanceViewFactoryChainContext agentInstanceContext, View originalChildView)
        {
            if (originalChildView is MergeView)
            {
                const string message = "Unexpected merge view as child of group-by view";
                Log.Error(".copySubViews " + message);
                throw new EPException(message);
            }

            if (!(originalChildView is CloneableView))
            {
                throw new EPException("Unexpected error copying subview " + originalChildView.GetType().FullName);
            }
            var cloneableView = (CloneableView)originalChildView;

            // Copy child node
            var copyChildView = cloneableView.CloneView();
            copyChildView.Parent = groupView;

            // Make the sub views for child copying from the original to the child
            CopySubViews(groupView.CriteriaExpressions, propertyNames, groupByValues, originalChildView, copyChildView,
                    agentInstanceContext);

            return copyChildView;
        }

        private static void CopySubViews(
            ExprNode[] criteriaExpressions,
            String[] propertyNames,
            Object groupByValues,
            View originalView,
            View copyView,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            foreach (var subView in originalView.Views)
            {
                // Determine if view is our merge view
                if (subView is MergeViewMarker)
                {
                    var mergeView = (MergeViewMarker)subView;
                    if (ExprNodeUtility.DeepEquals(mergeView.GroupFieldNames, criteriaExpressions, false))
                    {
                        if (mergeView.EventType != copyView.EventType)
                        {
                            // We found our merge view - install a new data merge view on top of it
                            var addPropertyView = new AddPropertyValueOptionalView(agentInstanceContext, propertyNames, groupByValues, mergeView.EventType);

                            // Add to the copied parent subview the view merge data view
                            copyView.AddView(addPropertyView);

                            // Add to the new merge data view the actual single merge view instance that clients may attached to
                            addPropertyView.AddView(mergeView);

                            // Add a parent view to the single merge view instance
                            mergeView.AddParentView(addPropertyView);
                        }
                        else
                        {
                            // Add to the copied parent subview the view merge data view
                            copyView.AddView(mergeView);

                            // Add a parent view to the single merge view instance
                            mergeView.AddParentView(copyView);
                        }

                        continue;
                    }
                }

                if (!(subView is CloneableView))
                {
                    throw new EPException("Unexpected error copying subview");
                }
                var cloneableView = (CloneableView)subView;
                var copiedChild = cloneableView.CloneView();
                copyView.AddView(copiedChild);

                // Make the sub views for child
                CopySubViews(criteriaExpressions, propertyNames, groupByValues, subView, copiedChild, agentInstanceContext);
            }
        }

        private Object GetGroupKey(EventBean theEvent)
        {
            _eventsPerStream[0] = theEvent;
            if (_criteriaEvaluators.Length == 1)
            {
                return _criteriaEvaluators[0].Evaluate(new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
            }

            var values = new Object[_criteriaEvaluators.Length];
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceContext);
            for (var i = 0; i < _criteriaEvaluators.Length; i++)
            {
                values[i] = _criteriaEvaluators[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(values);
        }

        internal static Object AddUpgradeToDequeIfPopulated(Object holder, EventBean theEvent)
        {
            if (holder == null)
            {
                return theEvent;
            }
            else if (holder is Deque<EventBean>)
            {
                var deque = (Deque<EventBean>)holder;
                deque.Add(theEvent);
                return deque;
            }
            else
            {
                var deque = new ArrayDeque<EventBean>(4);
                deque.Add((EventBean)holder);
                deque.Add(theEvent);
                return deque;
            }
        }

        internal static EventBean[] ConvertToArray(Object eventOrDeque)
        {
            if (eventOrDeque == null)
            {
                return null;
            }
            if (eventOrDeque is EventBean)
            {
                return new EventBean[] { (EventBean)eventOrDeque };
            }
            return ((ICollection<EventBean>)eventOrDeque).ToArray();
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
