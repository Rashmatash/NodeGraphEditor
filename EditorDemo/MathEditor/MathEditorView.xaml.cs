// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using EditorDemo.MathEditor.Nodes;
using NodeGraphEditor.Editors;
using NodeGraphEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace EditorDemo.MathEditor
{
    /// <summary>
    /// Interaction logic for MathEditorView.xaml
    /// </summary>
    public partial class MathEditorView : UserControl
    {
        private Point _TopLeft = new Point(0, 0);
        private NodeConnector _Connector = null;


        public MathEditorView()
        {
            InitializeComponent();
        }

        private void SetTopLeft(Point p)
        {
            var vm = DataContext as MathEditorViewModel;
            _TopLeft = p;
            var scale = 1.0 / vm.GraphEditor.ScaleFactor;
            _TopLeft.X *= scale;
            _TopLeft.Y *= scale;
            _TopLeft.X -= vm.GraphEditor.PanOffset.X;
            _TopLeft.Y -= vm.GraphEditor.PanOffset.Y;
        }

        private void OnGraphEditor_Mouse_RBD(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if(!(nodePicker.DataContext is NodePickerViewModel))
                {
                    nodePicker.DataContext = new NodePickerViewModel(typeof(MathNode));
                }
                var pos = e.GetPosition(sender as UIElement);
                SetTopLeft(pos);
                nodePicker.Show(pos);
                e.Handled = true;
            }
        }

        private void OnNodeSelected(object sender, NodePickerEventArgs e)
        {
            nodePicker.Collapse();
            if (e == null) return;

            try
            {
                if (!(Activator.CreateInstance(e.NodeType) is Node newNode))
                    throw new Exception($"Error: failed to create instance of node type {e.NodeDescription}");
                var vm = DataContext as MathEditorViewModel;
                vm.GraphEditor.AddNode(newNode);
                GetGroup(_TopLeft.X, _TopLeft.Y)?.AddChild(newNode);
                newNode.TopLeft = _TopLeft;

                ConnectNodes(newNode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private GroupNode GetGroup(double x, double y)
        {
            var group = this.FindVisualChild<GroupNodeView>();
            if (group == null) return null;

            var canvas = group.FindVisualParent<Canvas>();
            if (canvas == null) return null;

            List<GroupNode> groups = new List<GroupNode>();
            foreach (FrameworkElement child in canvas.Children)
            {
                if (child.DataContext is GroupNode groupVM)
                {
                    var top = groupVM.TopLeft.Y;
                    var left = groupVM.TopLeft.X;
                    var bottom = top + child.RenderSize.Height;
                    var right = left + child.RenderSize.Width;

                    if (x >= left && x <= right && y >= top && y <= bottom)
                    {
                        groups.Add(groupVM);
                    }
                }
            }

            var graphVM = (DataContext as MathEditorViewModel).GraphEditor;
            int index = -1;
            foreach (var groupVM in groups)
            {
                var newIndex = graphVM.Groups.IndexOf(groupVM);
                if (newIndex > index) index = newIndex;
            }

            if (index > -1)
            {
                return graphVM.Groups[index];
            }

            return null;
        }

        private void ConnectNodes(Node newNode)
        {
            if (_Connector is InputConnector)
            {
                var outputs = newNode.GetOutputs();
                foreach (var output in outputs)
                {
                    if (TypeConversion.CanConvert(output.DataType, _Connector.DataType))
                    {
                        (_Connector as InputConnector).AddTransitionTo(output);
                        break;
                    }
                }
            }
            else if (_Connector is OutputConnector)
            {
                var inputs = newNode.GetInputs();
                foreach (var input in inputs)
                {
                    if (TypeConversion.CanConvert(_Connector.DataType, input.DataType))
                    {
                        input.AddTransitionTo(_Connector as OutputConnector);
                        break;
                    }
                }
            }
            _Connector = null;
        }

        private void OnGraphEditor_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NodeConnectorView)) &&
                e.Data.GetData(typeof(NodeConnectorView)) is NodeConnectorView connector)
            {
                _Connector = connector.DataContext as NodeConnector;
                var pos = e.GetPosition(sender as UIElement);
                SetTopLeft(pos);
                nodePicker.Show(pos);
            }
        }
    }
}
