// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Collections.Specialized;
using NodeGraphEditor.Helpers;

namespace NodeGraphEditor.Editors
{
    public class Transition : BaseViewModel
    {
        private Point _Start;
        public Point Start
        {
            get { return _Start; }
            set
            {
                if (_Start != value)
                {
                    _Start = value;
                    OnPropertyChanged(nameof(Start));
                }
            }
        }

        private Point _End;
        public Point End
        {
            get { return _End; }
            set
            {
                if (_End != value)
                {
                    _End = value;
                    OnPropertyChanged(nameof(End));
                }
            }
        }

        private Point _CP1;
        public Point CP1
        {
            get { return _CP1; }
            set
            {
                if (_CP1 != value)
                {
                    _CP1 = value;
                    OnPropertyChanged(nameof(CP1));
                }
            }
        }

        private Point _CP2;
        public Point CP2
        {
            get { return _CP2; }
            set
            {
                if (_CP2 != value)
                {
                    _CP2 = value;
                    OnPropertyChanged(nameof(CP2));
                }
            }
        }

        public InputConnector Input { get; private set; }
        public OutputConnector Output { get; private set; }

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

        public bool IsValid => (Output.DataType == Input.DataType) ||
                    (Input.DataType == typeof(object)) ||
                    TypeConversion.CanConvert(Output.DataType, Input.DataType);

        public Transition(OutputConnector output, InputConnector input)
        {
            Input = input;
            Output = output;

            Output.PropertyChanged += AdjustStart;
            Input.PropertyChanged += AdjustEnd;
            Input.Transitions.CollectionChanged += DetachWhenRemoved;

            Start = output.AnchorPoint;
            End = input.AnchorPoint;
            CalculateControlPoints();
        }

        private void AdjustStart(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Output.AnchorPoint))
            {
                Start = Output.AnchorPoint;
                CalculateControlPoints();
            }
            else if (e.PropertyName == nameof(Output.DataType))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private void AdjustEnd(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Input.AnchorPoint))
            {
                End = Input.AnchorPoint;
                CalculateControlPoints();
            }
        }

        private void DetachWhenRemoved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(this))
            {
                Output.PropertyChanged -= AdjustStart;
                Input.PropertyChanged -= AdjustEnd;
                Input.Transitions.CollectionChanged -= DetachWhenRemoved;
            }
        }

        private void CalculateControlPoints()
        {
            double x = Math.Max(50, Math.Abs((Start.X - End.X) * 0.5));
            double y = (Input.ParentNode == Output.ParentNode) ? 50.0 : 0.0;
            CP1 = new Point(Start.X + x, Start.Y + y);
            CP2 = new Point(End.X - x, End.Y + y);
        }
    }
}
