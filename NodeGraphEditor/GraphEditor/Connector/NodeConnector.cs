// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Diagnostics.Debug;
using System.ComponentModel;
using NodeGraphEditor.Helpers;

namespace NodeGraphEditor.Editors
{
    public class NodeConnector : BaseViewModel
    {
        public Node ParentNode { get; private set; }
        private Type _DataType;
        public Type DataType
        {
            get { return _DataType; }
            set
            {
                if (_DataType != value)
                {
                    _DataType = value;
                    OnPropertyChanged(nameof(DataType));
                }
            }
        }

        private Point anchorPoint;
        public Point AnchorPoint
        {
            get { return anchorPoint; }
            set
            {
                if (anchorPoint != value)
                {
                    anchorPoint = value;
                    OnPropertyChanged(nameof(AnchorPoint));
                }
            }
        }

        public virtual bool IsConnected { get; }

        public NodeConnector(Node parent, Type type)
        {
            Assert(parent != null);
            ParentNode = parent;
            DataType = type;
        }
    }

    public class InputConnector : NodeConnector
    {
        public event EventHandler ValueChanged;

        public int MaxConnections { get; } = 0; // 0 means unlimited number of connections

        public override bool IsConnected => (Transitions.Count > 0);

        public ObservableCollection<Transition> Transitions
        { get; } = new ObservableCollection<Transition>();

        public InputConnector(Node parent, Type type, int numConnections = 1)
            : base(parent, type)
        {
            MaxConnections = numConnections;
        }

        public void AddTransitionTo(OutputConnector output)
        {
            Assert(output != null);
            if (MaxConnections == 1 && Transitions.Count == 1)
            {
                RemoveTransitionTo(Transitions.First().Output);
            }

            if ((MaxConnections == 0) || (Transitions.Count < MaxConnections))
            {
                // Check if output and input data types are compatible
                if (output.DataType == DataType || DataType == typeof(object) ||
                    TypeConversion.CanConvert(output.DataType, DataType))
                {
                    // Check if there is already a transition
                    // to this node. If so, do nothing.
                    if (Transitions.FirstOrDefault(x => x.Output.ParentNode == output.ParentNode) != null)
                    {
                        return;
                    }
                    // NOTE(arash): Don't change the order of following statements!
                    var transition = ParentNode.CreateTransition(output, this);
                    Transitions.Add(transition);
                    ParentNode.Transitions.Add(transition);
                    Transitions.Last().Output.PropertyChanged += OutputChanged;
                    ++Transitions.Last().Output.NumConnections;
                    ValueChanged?.Invoke(this, new EventArgs());

                }
            }
            OnPropertyChanged(nameof(IsConnected));
        }

        public void RemoveTransitionTo(OutputConnector output)
        {
            if (output != null)
            {
                foreach (var item in Transitions)
                {
                    if (item.Output.ParentNode == output.ParentNode)
                    {
                        // NOTE(arash): Don't change the order of following statements!
                        Transitions.Remove(item);
                        ParentNode.Transitions.Remove(item);
                        --item.Output.NumConnections;
                        item.Output.PropertyChanged -= OutputChanged;
                        ValueChanged?.Invoke(this, new EventArgs());

                        break;
                    }
                }
            }
            OnPropertyChanged(nameof(IsConnected));
        }

        private void OutputChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OutputConnector.Value))
            {
                ValueChanged?.Invoke(this, e);
            }
        }
    }

    public class OutputConnector : NodeConnector
    {
        public override bool IsConnected => (_NumConnections > 0);

        private uint _NumConnections;
        public uint NumConnections
        {
            get { return _NumConnections; }
            set
            {
                if (_NumConnections != value)
                {
                    _NumConnections = value;
                    OnPropertyChanged(nameof(NumConnections));
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (_Value != value)
                {
                    _Value = TypeConversion.ConvertTo(value, DataType);
                    if (_Value == null)
                    {
                        try { _Value = Activator.CreateInstance(DataType); } catch { }
                    }
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
        public T GetValue<T>()
        {
            T value = (_Value == null) ? default : (T)TypeConversion.ConvertTo(_Value, typeof(T));
            return (value != null) ? value : default;
        }

        public OutputConnector(Node parent, Type type) : base(parent, type) { }
    }
}
