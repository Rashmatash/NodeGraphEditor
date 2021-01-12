// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Xml;
using static System.Diagnostics.Debug;

namespace NodeGraphEditor.Editors
{
    abstract public class Node : BaseViewModel
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        private Point _TopLeft = new Point(0, 0);
        public Point TopLeft
        {
            get { return _TopLeft; }
            set
            {
                if (_TopLeft != value)
                {
                    _TopLeft = value;
                    OnPropertyChanged(nameof(TopLeft));
                }
            }
        }

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

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    OnPropertyChanged(nameof(Description));
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

        private bool _IsValid = true;
        public bool IsValid
        {
            get { return _IsValid; }
            set
            {
                if (_IsValid != value)
                {
                    _IsValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private bool _IsRemovable = true;
        public bool IsRemovable
        {
            get { return _IsRemovable; }
            set
            {
                if (_IsRemovable != value)
                {
                    _IsRemovable = value;
                    OnPropertyChanged(nameof(IsRemovable));
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

        public ObservableCollection<Transition> Transitions
        { get; } = new ObservableCollection<Transition>();

        public abstract List<InputConnector> GetInputs();
        public abstract List<OutputConnector> GetOutputs();

        public virtual Transition CreateTransition(OutputConnector output, InputConnector input)
        {
            return new Transition(output, input);
        }

        public void ClearConnectionsTo(List<OutputConnector> outputs)
        {
            foreach (var input in GetInputs())
            {
                foreach (var output in outputs)
                {
                    input.RemoveTransitionTo(output);
                }
            }
        }

        protected void XMLSerializationErrorMessage(bool isDeserializing, string exceptionMessage = "")
        {
            var de = isDeserializing ? "de" : "";
            var msg = $"failed to {de}serialize node {Name}!";
            WriteLine(msg);
            WriteLine(exceptionMessage);
        }

        public virtual XmlNode ToXML(XmlDocument xmlDoc)
        {
            Contract.Assert(xmlDoc != null);
            try
            {
                var element = xmlDoc.CreateElement(nameof(Node));

                // Type name
                var attribute = xmlDoc.CreateAttribute("TypeName");
                attribute.Value = GetType().FullName;
                element.Attributes.Append(attribute);

                // Name
                attribute = xmlDoc.CreateAttribute(nameof(Name));
                attribute.Value = Name;
                element.Attributes.Append(attribute);

                // NOTE: don't use nameof(Id), nameof(X), etc. so the attribute names won't
                //       collide with node properties with the same name down the hierarchy.
                // Id
                attribute = xmlDoc.CreateAttribute("NodeId");
                attribute.Value = Id.ToString();
                element.Attributes.Append(attribute);

                // X
                attribute = xmlDoc.CreateAttribute(nameof(TopLeft) + "X");
                attribute.Value = ((int)TopLeft.X).ToString();
                element.Attributes.Append(attribute);

                // Y
                attribute = xmlDoc.CreateAttribute(nameof(TopLeft) + "Y");
                attribute.Value = ((int)TopLeft.Y).ToString();
                element.Attributes.Append(attribute);

                // Description
                if (!string.IsNullOrEmpty(Description))
                {
                    attribute = xmlDoc.CreateAttribute(nameof(Description));
                    attribute.Value = Description;
                    element.Attributes.Append(attribute);
                }

                // IsRemovable
                attribute = xmlDoc.CreateAttribute(nameof(IsRemovable));
                attribute.Value = IsRemovable.ToString();
                element.Attributes.Append(attribute);

                // ParentGroup Id
                if (ParentGroup != null)
                {
                    attribute = xmlDoc.CreateAttribute(nameof(ParentGroup));
                    attribute.Value = ParentGroup.Id.ToString();
                    element.Attributes.Append(attribute);
                }

                // NOTE: Transitions are saved separately by the graph editor.
                return element;
            }
            catch (Exception ex)
            {
                XMLSerializationErrorMessage(false, ex.Message);
                return null;
            }
        }

        public static Node CreateFromXML(XmlElement node, GraphEditor graphEditor)
        {
            Contract.Assert(node != null);
            try
            {
                var typeName = node.GetAttribute("TypeName");
                var type = Type.GetType(typeName);
                if (!(Activator.CreateInstance(type) is Node newNode))
                    throw new Exception($"Error: failed to create instance of node type {typeName}");

                newNode.FromXML(node, graphEditor);
                return newNode;
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
                WriteLine("Error: failed to deserialize node");
                return null;
            }
        }

        public virtual void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            Contract.Assert(node != null);
            try
            {
                Name = node.GetAttribute(nameof(Name));
                Id = Guid.Parse(node.GetAttribute("NodeId"));

                var x = double.Parse(node.GetAttribute(nameof(TopLeft) + "X"));
                var y = double.Parse(node.GetAttribute(nameof(TopLeft) + "Y"));
                TopLeft = new Point(x, y);

                Description = node.GetAttribute(nameof(Description));
                IsRemovable = bool.Parse(node.GetAttribute(nameof(IsRemovable)));

                var id = node.GetAttribute(nameof(ParentGroup));
                if (!string.IsNullOrEmpty(id))
                {
                    var ParentGroupId = Guid.Parse(node.GetAttribute(nameof(ParentGroup)));
                    var parentGroup = graphEditor.Groups.FirstOrDefault(g => g.Id == ParentGroupId);
                    Application.Current.Dispatcher.Invoke(new Action(() => parentGroup.AddChild(this)));
                }

                // NOTE: Transitions are loaded later by the graph editor
            }
            catch (Exception ex)
            {
                XMLSerializationErrorMessage(true, ex.Message);
            }
        }
    }
}
