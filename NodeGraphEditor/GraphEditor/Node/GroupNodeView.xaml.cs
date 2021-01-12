// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using NodeGraphEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public partial class GroupNodeView : UserControl
    {
        public GroupNodeView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            UpdateDimensions();

            var vm = DataContext as GroupNode;

            vm.ChildNodes.CollectionChanged += UpdateView;
            vm.ChildGroups.CollectionChanged += UpdateView;

            foreach (var node in vm.ChildNodes)
            {
                node.PropertyChanged += UpdateView;
            }

            foreach (var group in vm.ChildGroups)
            {
                group.PropertyChanged += UpdateView;
            }
        }

        private void UpdateView(object sender, PropertyChangedEventArgs e)
        {
            dynamic element = sender;

            if (e.PropertyName == nameof(element.TopLeft) ||
                e.PropertyName == nameof(element.Width) ||
                e.PropertyName == nameof(element.Height))
            {
                UpdateDimensions();
            }
        }

        private void UpdateView(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (BaseViewModel vm in e.NewItems)
                {
                    vm.PropertyChanged += UpdateView;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BaseViewModel vm in e.OldItems)
                {
                    vm.PropertyChanged -= UpdateView;
                }
            }

            UpdateDimensions();
        }

        private void UpdateDimensions()
        {
            var vm = DataContext as GroupNode;
            if (vm == null || vm.IsMoving ||
            (vm.ChildGroups.Count == 0 && vm.ChildNodes.Count == 0)) return;

            var graph = this.FindVisualParent<GraphEditorView>();
            if (graph == null) return;

            var nodeView = graph.FindVisualChild<NodeView>();
            if (nodeView == null) return;

            var nodeCanvas = nodeView.FindVisualParent<Canvas>();
            if (nodeCanvas == null) return;


            var groupCanvas = this.FindVisualParent<Canvas>();
            if (groupCanvas == null) return;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            CalculateBoundingBox(ref minX, ref minY, ref maxX, ref maxY, nodeCanvas);
            minX -= 20.0; minY -= 20.0; maxX += 20.0; maxY += 20.0; // set margin for nodes in the group
            CalculateBoundingBox(ref minX, ref minY, ref maxX, ref maxY, groupCanvas);
            minX -= 5.0; minY -= 24.0; maxX += 5.0; maxY -= 20.0; // set margin for subgroups


            vm.TopLeft = new Point(minX, minY);
            vm.Width = (maxX - minX);
            vm.Height = (maxY - minY);

            UpdateLayout();

            if (vm.ParentGroup != null)
            {
                foreach (FrameworkElement group in groupCanvas.Children)
                {
                    if (group.DataContext == vm.ParentGroup)
                    {
                        group.FindVisualChild<GroupNodeView>()?.UpdateDimensions();
                    }
                }
            }
        }

        private void CalculateBoundingBox(ref double minX, ref double minY,
            ref double maxX, ref double maxY, Canvas canvas)
        {
            var vm = DataContext as GroupNode;
            foreach (FrameworkElement child in canvas.Children)
            {
                dynamic nvm = child.DataContext;

                if (nvm.ParentGroup == vm) // if the element is part of this group
                {
                    var x = nvm.TopLeft.X;
                    var y = nvm.TopLeft.Y;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);

                    maxX = Math.Max(maxX, x + child.ActualWidth);
                    maxY = Math.Max(maxY, y + child.ActualHeight);
                }
            }
        }
    }
}
