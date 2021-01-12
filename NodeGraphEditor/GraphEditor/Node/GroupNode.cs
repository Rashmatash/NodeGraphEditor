// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using static System.Diagnostics.Debug;

namespace NodeGraphEditor.Editors
{
    public class GroupNode : BaseViewModel
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    _Color.A = 255;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private bool _IsMoving;
        public bool IsMoving
        {
            get { return _IsMoving; }
            set
            {
                if (_IsMoving != value)
                {
                    foreach (var group in ChildGroups)
                    {
                        group.IsMoving = value;
                    }
                    _IsMoving = value;
                    OnPropertyChanged(nameof(IsMoving));
                }
            }
        }

        private Point _TopLeft;
        public Point TopLeft
        {
            get { return _TopLeft; }
            set
            {
                if (_TopLeft != value)
                {
                    if (IsMoving)
                    {
                        var offset = _TopLeft - value;
                        foreach (var node in ChildNodes)
                        {
                            node.TopLeft =
                                new Point(node.TopLeft.X - offset.X, node.TopLeft.Y - offset.Y);
                        }
                        foreach (var group in ChildGroups)
                        {
                            if (!group.IsSelected)
                            {
                                group.TopLeft =
                                    new Point(group.TopLeft.X - offset.X, group.TopLeft.Y - offset.Y);
                            }
                        }
                    }
                    _TopLeft = value;
                    OnPropertyChanged(nameof(TopLeft));
                }
            }
        }

        private double _Width = 128;
        public double Width
        {
            get { return _Width; }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private GroupNode _ParentGroup;
        public GroupNode ParentGroup
        {
            get { return _ParentGroup; }
            set
            {
                if (_ParentGroup != value)
                {
                    _ParentGroup = value;
                    OnPropertyChanged(nameof(ParentGroup));
                }
            }
        }

        public ObservableCollection<GroupNode> ChildGroups
        { get; } = new ObservableCollection<GroupNode>();

        public ObservableCollection<Node> ChildNodes
        { get; } = new ObservableCollection<Node>();

        public void AddChild(Node node)
        {
            if (node.ParentGroup == this)
            {
                Assert(ChildNodes.Contains(node));
                return;
            }
            else if (node.ParentGroup != null)
            {
                node.ParentGroup.RemoveChild(node);
            }
            Assert(!ChildNodes.Contains(node));
            node.ParentGroup = this;
            ChildNodes.Add(node);
        }

        public void AddChild(GroupNode group)
        {
            if (group.ParentGroup == this)
            {
                Assert(ChildGroups.Contains(group));
                return;
            }
            else if (group.ParentGroup != null)
            {
                group.ParentGroup.RemoveChild(group);
            }
            Assert(!ChildGroups.Contains(group));
            group.ParentGroup = this;
            ChildGroups.Add(group);
        }

        public void RemoveChild(Node node)
        {
            Assert(node.ParentGroup == this);
            Assert(ChildNodes.Contains(node));
            node.ParentGroup = null;
            ChildNodes.Remove(node);
        }

        public void RemoveChild(GroupNode group)
        {
            Assert(group.ParentGroup == this);
            Assert(ChildGroups.Contains(group));
            group.ParentGroup = null;
            ChildGroups.Remove(group);
        }

        public GroupNode()
        {
            Color = Colors.LightGray;
            Name = "Group";
        }

        public GroupNode(IEnumerable<Node> nodes, IEnumerable<GroupNode> groups) : this()
        {
            Assert(nodes.Count() > 0 || groups.Count() > 0);

            foreach (var node in nodes) AddChild(node);
            foreach (var group in groups) AddChild(group);
        }

        public virtual XmlNode ToXML(XmlDocument xmlDoc)
        {
            Contract.Assert(xmlDoc != null);
            try
            {
                var element = xmlDoc.CreateElement(GetType().Name);
                // Name
                var attribute = xmlDoc.CreateAttribute(nameof(Name));
                attribute.Value = Name;
                element.Attributes.Append(attribute);

                // Id
                attribute = xmlDoc.CreateAttribute(nameof(Id));
                attribute.Value = Id.ToString();
                element.Attributes.Append(attribute);

                // Color
                attribute = xmlDoc.CreateAttribute(nameof(Color));
                attribute.Value = Color.ToString();
                element.Attributes.Append(attribute);

                // ParentGroup
                if (ParentGroup != null)
                {
                    attribute = xmlDoc.CreateAttribute(nameof(ParentGroup));
                    attribute.Value = ParentGroup.Id.ToString();
                    element.Attributes.Append(attribute);
                }

                return element;
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
                WriteLine("Error: failed to serialize group node " + Name);
                return null;
            }
        }

        public void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            Contract.Assert(node != null);
            try
            {
                Name = node.GetAttribute(nameof(Name));
                Id = Guid.Parse(node.GetAttribute(nameof(Id)));
                Color = (Color)ColorConverter.ConvertFromString(node.GetAttribute(nameof(Color)));
                // NOTE: this only works because group nodes are sorted back to front!
                var id = node.GetAttribute(nameof(ParentGroup));
                if (!string.IsNullOrEmpty(id))
                {
                    var ParentGroupId = Guid.Parse(node.GetAttribute(nameof(ParentGroup)));
                    var parentGroup = graphEditor.Groups.FirstOrDefault(g => g.Id == ParentGroupId);
                    parentGroup.AddChild(this);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
                WriteLine("Error: failed to deserialize group node " + Name);
            }
        }
    }
}
