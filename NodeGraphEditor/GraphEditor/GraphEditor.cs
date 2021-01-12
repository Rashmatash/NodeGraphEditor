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
using System.Xml;

namespace NodeGraphEditor.Editors
{
    public class GraphEditor : BaseViewModel
    {
        private Point _PanOffset = new Point(0, 0);
        public Point PanOffset
        {
            get { return _PanOffset; }
            set
            {
                if (_PanOffset != value)
                {
                    _PanOffset = value;
                    OnPropertyChanged(nameof(PanOffset));
                }
            }
        }

        private double _ScaleFactor = 1.0;
        public double ScaleFactor
        {
            get { return _ScaleFactor; }
            set
            {
                if (_ScaleFactor != value)
                {
                    _ScaleFactor = value;
                    OnPropertyChanged(nameof(ScaleFactor));
                }
            }
        }

        public ObservableCollection<GroupNode> Groups
        { get; } = new ObservableCollection<GroupNode>();

        public ObservableCollection<GroupNode> GroupSelection
        { get; } = new ObservableCollection<GroupNode>();

        public GroupNode SelectedGroup
        { get { return GroupSelection.FirstOrDefault(); } }

        public ObservableCollection<Node> Nodes
        { get; } = new ObservableCollection<Node>();

        public ObservableCollection<Node> NodeSelection
        { get; } = new ObservableCollection<Node>();

        public Node SelectedNode
        { get { return NodeSelection.FirstOrDefault(); } }

        private Transition _SelectedTransition;
        public Transition SelectedTransition
        {
            get { return _SelectedTransition; }
            set
            {
                if (_SelectedTransition != value)
                {
                    _SelectedTransition = value;
                    OnPropertyChanged(nameof(SelectedTransition));
                }
            }
        }

        public GraphEditor()
        {
            GroupSelection.CollectionChanged += (s, e) => OnPropertyChanged(nameof(SelectedGroup));
            NodeSelection.CollectionChanged += (s, e) => OnPropertyChanged(nameof(SelectedNode));
        }

        public void ResetGraph()
        {
            SelectedTransition = null;
            GroupSelection.Clear();
            foreach (var group in Groups)
            {
                RemoveGroupNode(group);
            }
            NodeSelection.Clear();
            foreach (var node in Nodes)
            {
                RemoveNode(node);
            }
        }

        public void AddNode(Node node)
        {
            Contract.Assert(node != null);
            Nodes.Add(node);
        }

        public void RemoveNode(Node node)
        {
            foreach (var item in Nodes)
            {
                if (item != node)
                {
                    item.ClearConnectionsTo(node.GetOutputs());
                    node.ClearConnectionsTo(item.GetOutputs());
                }
            }

            node.ParentGroup?.RemoveChild(node);
            Nodes.Remove(node);
            node.OnPropertyChanged(nameof(node.TopLeft));
        }

        public void AddGroupNode(GroupNode group)
        {
            Groups.Add(group);
            // Sort group nodes so the child nodes are in front of the parent nodes.
            for (int i = 0; i < Groups.Count - 1; ++i)
            {
                for (int j = i + 1; j < Groups.Count; ++j)
                {
                    if (Groups[i].ParentGroup == Groups[j])
                    {
                        Groups.Move(j, i);
                    }
                }
            }
        }

        public void RemoveGroupNode(GroupNode group)
        {
            group.ParentGroup?.RemoveChild(group);
            Groups.Remove(group);
        }

        public XmlDocument ToXML()
        {
            try
            {
                var doc = new XmlDocument();
                var graph = doc.CreateElement("Graph");
                doc.AppendChild(graph);

                // Groups
                var groups = doc.CreateElement(nameof(Groups));
                graph.AppendChild(groups);
                foreach (var group in Groups)
                {
                    groups.AppendChild(group.ToXML(doc));
                }

                // Nodes
                var nodes = doc.CreateElement(nameof(Nodes));
                graph.AppendChild(nodes);
                foreach (var node in Nodes)
                {
                    nodes.AppendChild(node.ToXML(doc));
                }

                // Transitions
                var transitions = doc.CreateElement("Transitions");
                graph.AppendChild(transitions);
                foreach (var n in Nodes)
                {
                    var node = doc.CreateElement(nameof(Node));
                    transitions.AppendChild(node);

                    var attribute = doc.CreateAttribute(nameof(n.Id));
                    attribute.Value = n.Id.ToString();
                    node.Attributes.Append(attribute);

                    foreach (var i in n.GetInputs())
                    {
                        var input = doc.CreateElement("Input");
                        node.AppendChild(input);

                        foreach (var t in i.Transitions)
                        {
                            var transition = doc.CreateElement(nameof(Transition));
                            input.AppendChild(transition);

                            attribute = doc.CreateAttribute(nameof(Node));
                            attribute.Value = t.Output.ParentNode.Id.ToString();
                            transition.Attributes.Append(attribute);

                            attribute = doc.CreateAttribute("OutputIndex");
                            attribute.Value = t.Output.ParentNode.GetOutputs().IndexOf(t.Output).ToString();
                            transition.Attributes.Append(attribute);
                        }
                    }
                }

                return doc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to serialize graph");
                return null;
            }
        }

        public async Task FromXML(XmlDocument doc)
        {
            Contract.Assert(doc != null);
            try
            {
                ResetGraph();
                var graph = doc.SelectSingleNode("Graph") as XmlElement;

                // Create groups
                {
                    var groups = (graph.SelectSingleNode(nameof(Groups)) as XmlElement)?.ChildNodes;
                    foreach (XmlElement g in groups)
                    {
                        var group = new GroupNode();
                        group.FromXML(g, this);
                        Groups.Add(group);
                    }
                }

                // Create nodes
                {
                    var nodes = (graph.SelectSingleNode(nameof(Nodes)) as XmlElement)?.ChildNodes;
                    foreach (XmlElement n in nodes)
                    {
                        Node node = null;
                        await Task.Run(new Action(() => { node = Node.CreateFromXML(n, this); }));
                        Debug.Assert(node != null);
                        Nodes.Add(node);
                    }

                    // Trigger group nodes to update their dimension
                    foreach (var node in Nodes)
                    {
                        node.OnPropertyChanged(nameof(node.TopLeft));
                    }
                }

                // Create transitions
                {
                    var nodes = (graph.SelectSingleNode("Transitions") as XmlElement)?.ChildNodes;
                    foreach (XmlElement n in nodes)
                    {
                        var id = Guid.Parse(n.GetAttribute(nameof(Node.Id)));
                        var node = Nodes.FirstOrDefault(x => x.Id == id);
                        var nodeInputs = node.GetInputs();
                        var inputs = n.ChildNodes;
                        Debug.Assert(nodeInputs.Count == inputs.Count);
                        for (int i = 0; i < inputs.Count; ++i)
                        {
                            foreach (XmlElement t in inputs[i].ChildNodes)
                            {
                                var targetNodeId = Guid.Parse(t.GetAttribute(nameof(Node)));
                                int outputIndex = int.Parse(t.GetAttribute("OutputIndex"));
                                var output = Nodes.FirstOrDefault(x => x.Id == targetNodeId)
                                    .GetOutputs()[outputIndex];

                                nodeInputs[i].AddTransitionTo(output);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to deserialize graph");
            }
        }
    }
}
