// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraphEditor.Editors
{
    public class NodePickerEventArgs : EventArgs
    {
        public string NodeDescription { get; }
        public Type NodeType { get; }
        public NodePickerEventArgs(string desc, Type type)
        {
            NodeDescription = desc;
            NodeType = type;
        }
    }

    public partial class NodePickerView : UserControl
    {
        public event EventHandler<NodePickerEventArgs> NodeSelected;

        public NodePickerView()
        {
            InitializeComponent();
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tuple = (sender as ListBoxItem).DataContext as Tuple<string, Type>;
            NodeSelected?.Invoke(this, new NodePickerEventArgs(tuple.Item1, tuple.Item2));
            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NodeSelected?.Invoke(this, null);
            }
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Focus();
            }
            (DataContext as NodePickerViewModel).SearchPhrase = "";
        }

        public void Show(Point pos)
        {
            Canvas.SetLeft(pickerBorder, pos.X);
            Canvas.SetTop(pickerBorder, pos.Y);
            Visibility = Visibility.Visible;
        }

        public void Collapse()
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnCanvas_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            Collapse();
        }
    }
}
