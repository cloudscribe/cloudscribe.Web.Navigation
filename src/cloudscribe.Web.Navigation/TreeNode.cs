// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2019-02-15
// 


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace cloudscribe.Web.Navigation
{
    [Serializable()]
    public class TreeNode<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public TreeNode(T value)
        {
            _value = value;
        }

        [JsonConstructor]
        public TreeNode(T value, List<TreeNode<T>> children)
        {
            _value    = value;
            _children = children;  // JsonConvert won't deserialize to a ReadOnlyCollection, so setting this here.
        }

        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        // JsonConvert throws an error if an object has a reference to its parent... 
        // jk - This property is probably redundant now...
        [JsonIgnore]
        public TreeNode<T> Parent { get; private set; } = null;

        // jk - Collated parentage in a form that is serializable without recursion problems
        public List<T> ParentValueChain { get; set; } = new List<T>();

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value); 
            node.ParentValueChain.AddRange(this.ParentValueChain);
            node.ParentValueChain.Add(this.Value);
            node.Parent = this; 

            _children.Add(node);
            return node;
        }

        public TreeNode<T> AddChild(TreeNode<T> node)
        {
            node.ParentValueChain.AddRange(this.ParentValueChain);
            node.ParentValueChain.Add(this.Value);
            node.Parent = this; 

            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }
        
        public TreeNode<T> Find(Func<TreeNode<T>, bool> predicate)
        {
            if(predicate(this)) { return this; }
            foreach(var n in Children)
            {
                var found = n.Find(predicate);
                if (found != null) return found;
            }

            return null;
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Union(_children.SelectMany(x => x.Flatten()));
        }

        public TreeNode<T> GetParent()
        {
            var parentChainCount = ParentValueChain.Count;
            if (parentChainCount == 0) return null;

            var parentChainValue = ParentValueChain[parentChainCount - 1];
            var parentNode = new TreeNode<T>(parentChainValue);

            // ordering is preserved during List manipulation
            parentNode.ParentValueChain.AddRange(ParentValueChain);
            parentNode.ParentValueChain.RemoveAt(parentChainCount - 1);

            return parentNode;
        }
    }
}
