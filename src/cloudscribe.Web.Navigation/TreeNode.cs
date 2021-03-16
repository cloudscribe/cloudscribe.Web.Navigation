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
        private TreeNode<T> _parent;

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
        // so this never gets serialized
        [JsonIgnore]
        public TreeNode<T> Parent
        {
            get 
            { 
                if (_parent == null) _parent = GetParent();
                return _parent;
            } 
            set
            {
                _parent = value;
            }
        } 


        public string ParentKey { get; set; }

        // jk - Collated parentage in a form that is serializable without recursion errors
        public List<T> ParentValueChain { get; set; } = new List<T>();

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value);

            // set appropriate parentage on the new node.
            // As my child - you get all of my own parentage, plus me...
            node.ParentValueChain.AddRange(this.ParentValueChain);
            node.ParentValueChain.Add(this.Value);

            // this no longer gets cached - left here for legacy
            node.Parent = this;

            // node.ParentKey = (this.Value as NavigationNode).Key;

            _children.Add(node);
            return node;
        }

        public TreeNode<T> AddChild(TreeNode<T> node)
        {
            // When adding a pre-existing child node to the tree, 
            // the receiving parent node takes control of re-asserting the
            // parentage lineage onto the newly added node plus all of its children
            
            // As my child - you get all of my own parentage, plus me...
            node.ParentValueChain = new List<T>(this.ParentValueChain);
            node.ParentValueChain.Add(this.Value);

            // and likewise recursive through the children of the newly added child
            SetChildParentage(node);

            // this no longer gets cached - left here for legacy
            node.Parent = this;

            // node.ParentKey = (this.Value as NavigationNode).Key;

            _children.Add(node);
            return node;
        }


        private void SetChildParentage(TreeNode<T> parent)
        {
            foreach (var ch in parent.Children)
            {
                ch.ParentValueChain = new List<T>(parent.ParentValueChain);
                ch.ParentValueChain.Add(parent.Value);
                SetChildParentage(ch);
            }
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

        /// <summary>
        /// Re-create parent node from the collated ParentValueChain
        /// </summary>
        public TreeNode<T> GetParent()
        {
            var parentChainCount = ParentValueChain.Count;
            if (parentChainCount == 0) return null;

            var parentChainValue = ParentValueChain[parentChainCount - 1];
            var parentNode = new TreeNode<T>(parentChainValue);

            // need to re-create the upwards parentage on this new parentNode
            // (ordering is preserved during List manipulation)

            // my parent has the same parent lineage as me, only without the last one
            // (which is the parent itself)
            parentNode.ParentValueChain = new List<T>(this.ParentValueChain);
            parentNode.ParentValueChain.RemoveAt(parentChainCount - 1);

            return parentNode;
        }
    }
}
