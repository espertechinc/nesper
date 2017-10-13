///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using com.espertech.esper.client;

namespace com.espertech.esper.linq
{
    public class StatementObservableCollection<T> : DisposableObservableCollection<T>
    {
        public const int MaxComparisonCount = 1000;

        /// <summary>
        /// Gets the function that converts the event bean into a properly formed object.
        /// </summary>
        public Func<EventBean, T> EventTransform { get; private set; }

        /// <summary>
        /// Gets the statement the collection is bound to.
        /// </summary>
        /// <value>The statement.</value>
        public EPStatement Statement { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dispose statement].
        /// </summary>
        /// <value><c>true</c> if [dispose statement]; otherwise, <c>false</c>.</value>
        public bool DisposeStatement { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="eventTransform">The event transform.</param>
        /// <param name="disposeStatement">if set to <c>true</c> [dispose statement].</param>
        public StatementObservableCollection(EPStatement statement, Func<EventBean, T> eventTransform, bool disposeStatement)
        {
            if (statement == null) {
                throw new ArgumentNullException("statement");
            }

            if (eventTransform == null) {
                throw new ArgumentNullException("eventTransform");
            }

            DisposeStatement = disposeStatement;
            EventTransform = eventTransform;
            Statement = statement;
            Statement.Events += OnEvent;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (Statement != null) {
                Statement.Events -= OnEvent;
                if ( DisposeStatement ) {
                    Statement.Dispose();
                }

                Statement = null;
            }
        }

        /// <summary>
        /// Called when an Update [event] occurs on the statement.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="com.espertech.esper.client.UpdateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnEvent(Object sender, UpdateEventArgs e)
        {
            var newEventSet = CreateTypedEventList(e.Statement);

            // Handle the case where the events are null or where there are
            // no events at all.
            if (newEventSet.Count == 0) {
                ClearItems();
                return;
            }

            // Create the typedEventList
            SetItems(newEventSet);
        }


        /// <summary>
        /// Sets the items.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        protected virtual void SetItems( List<T> itemList )
        {
            CheckReentrancy();

            // Compare the itemList against the items curerntly in the collection.
            // We would really prefer to generate the smallest event possible.

            switch (Count) {
                case 0:
                    SetItemsWhenEmpty(itemList);
                    return;
                case 1:
                    SetItemsWhenSingleOccupant(itemList);
                    return;
                default:
                    Items.Clear();

                    var count = itemList.Count;
                    for (var ii = 0; ii < count; ii++) {
                        Items.Add(itemList[ii]);
                    }

                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                    break;
            }
        }

        /// <summary>
        /// Sets the items when there is only a single item currently in the list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetItemsWhenSingleOccupant(IList<T> itemList)
        {
            CheckReentrancy();

            if (itemList.Count == 0)
            {
                var oldValue = Items[0];
                Items.RemoveAt(0);
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValue, 0));
            }
            else if (itemList.Count == 1)
            {
                if (Equals(itemList[0], Items[0]))
                {
                    return;
                }

                var oldValue = Items[0];
                var newValue = itemList[0];

                Items[0] = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newValue, oldValue, 0));
            }
            else
            {
                Items.Clear();

                var count = itemList.Count;
                for (var ii = 0; ii < count; ii++)
                {
                    Items.Add(itemList[ii]);
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Sets the items when there are no occupants in the list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetItemsWhenEmpty(IList<T> itemList)
        {
            CheckReentrancy();

            if ( itemList.Count == 0 ) {
                return;
            }

            var count = itemList.Count;
            for( var ii = 0 ; ii < count ; ii++ ) {
                Items.Add(itemList[ii]);
            }
                
            // Consider this a complete replacement.

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Creates the typed event list.
        /// </summary>
        /// <param name="eventList">The event list.</param>
        /// <returns></returns>
        protected virtual List<T> CreateTypedEventList( IEnumerable<EventBean> eventList )
        {
            var typedEventList = new List<T>();
            var transform = EventTransform;
            foreach( var eventBean in eventList ) {
                typedEventList.Add(transform.Invoke(eventBean));
            }

            return typedEventList;
        }
    }
}
