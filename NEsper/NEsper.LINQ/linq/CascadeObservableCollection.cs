///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace com.espertech.esper.runtime.client.linq
{
    public class CascadeObservableCollection<TSource, TTarget> : DisposableObservableCollection<TTarget>
    {
        /// <summary>
        /// Gets or sets a value indicating whether [dispose source].
        /// </summary>
        /// <value><c>true</c> if [dispose source]; otherwise, <c>false</c>.</value>
        public bool DisposeSource { get; private set; }

        /// <summary>
        /// Gets or sets the source collection.
        /// </summary>
        /// <value>The source collection.</value>
        public ObservableCollection<TSource> SourceCollection { get; private set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <value>The transform.</value>
        public Func<TSource, TTarget> Transform { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CascadeObservableCollection&lt;TTarget, TSource&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="disposeSource">if set to <c>true</c> [dispose source].</param>
        public CascadeObservableCollection(ObservableCollection<TSource> sourceCollection, Func<TSource, TTarget> transform, bool disposeSource)
        {
            DisposeSource = disposeSource;
            Transform = transform;
            SourceCollection = sourceCollection;
            SourceCollection.CollectionChanged += OnSourceCollectionChanged;

            foreach (var item in SourceCollection)
            {
                Add(transform.Invoke(item));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (DisposeSource)
            {
                if (SourceCollection != null)
                {
                    if (SourceCollection is IDisposable)
                    {
                        ((IDisposable) SourceCollection).Dispose();
                        SourceCollection = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when [source collection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var newItemList = new List<TTarget>();
                    for (int ii = e.NewItems.Count - 1; ii >= 0; ii--)
                    {
                        var newItem = (TSource) e.NewItems[ii];
                        var newItemTransform = Transform(newItem);
                        InsertItem(e.NewStartingIndex + ii, newItemTransform);
                        newItemList.Add(newItemTransform);
                    }

                    newItemList.Reverse();
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItemList, e.NewStartingIndex));
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var oldItemList = new List<TTarget>();
                    for (int ii = e.OldItems.Count - 1; ii >= 0; ii--)
                    {
                        oldItemList.Add(Items[ii]);
                        RemoveAt(e.OldStartingIndex + ii);
                    }

                    oldItemList.Reverse();
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItemList, e.OldStartingIndex));
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    for (int ii = e.NewItems.Count - 1; ii >= 0; ii--)
                    {
                        var newItem = Transform((TSource) e.NewItems[ii]);
                        var oldItem = Items[e.NewStartingIndex + ii];
                        Items[e.NewStartingIndex + ii] = newItem;
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                                                NotifyCollectionChangedAction.Replace,
                                                newItem,
                                                oldItem,
                                                e.NewStartingIndex + ii));
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    foreach (var item in SourceCollection)
                    {
                        Items.Add(Transform(item));
                    }

                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }
    }
}