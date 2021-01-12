// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using NodeGraphEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static System.Diagnostics.Debug;

namespace NodeGraphEditor.Editors
{
    public static class GraphEditorCommands
    {
        public static RoutedCommand DeleteNodeCommand = new RoutedCommand(
            "DeleteNodeCommand",
            typeof(GraphEditorView),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Delete)
            });
        public static RoutedCommand FocusNodeCommand = new RoutedCommand(
            "FocusNodeCommand",
            typeof(GraphEditorView),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Home)
            });
        public static RoutedCommand SelectAllCommand = new RoutedCommand(
           "SelectAllCommand",
           typeof(GraphEditorView),
           new InputGestureCollection()
           {
                new KeyGesture(Key.A, ModifierKeys.Control)
           });
        public static RoutedCommand GroupNodesCommand = new RoutedCommand(
            "GroupNodesCommand",
            typeof(GraphEditorView),
            new InputGestureCollection()
            {
                    new KeyGesture(Key.G, ModifierKeys.Control)
            });
        public static RoutedCommand UngroupNodesCommand = new RoutedCommand(
            "UngroupNodesCommand",
            typeof(GraphEditorView),
            new InputGestureCollection()
            {
                    new KeyGesture(Key.U, ModifierKeys.Control)
            });
    }
    
    public partial class GraphEditorView : UserControl
    {
        private Point gridClickPos = new Point(0, 0);
        private Rect viewport;
        private bool nodesMoved = false;
        private bool nodesMoving = false;
        private bool capturedLeft = false;
        private bool capturedRight = false;
        private bool boxSelectStarted = false;
        private Point boxTopLeft = new Point(0, 0);

        public GraphEditorView()
        {
            InitializeComponent();

            PreviewMouseDown += (s, e) => Focus();

            viewport = new Rect(bgImage.Viewport.X, bgImage.Viewport.Y,
                bgImage.Viewport.Width, bgImage.Viewport.Height);
        }

        private void CaptureMouse(IInputElement element)
        {
            int count = 0;
            if (capturedLeft) count++;
            if (capturedRight) count++;
            if (boxSelectStarted) count++;

            if (element != null && count == 1)
            {
                Mouse.Capture(element);
            }
            else if (Mouse.RightButton != MouseButtonState.Pressed &&
                    Mouse.LeftButton != MouseButtonState.Pressed)
                Mouse.Capture(null);
        }

        private void DeleteNodes(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            foreach (var node in vm.NodeSelection)
            {
                node.IsSelected = false;
                if (node.IsRemovable)
                {
                    vm.RemoveNode(node);
                }
            }
            vm.NodeSelection.Clear();
        }

        private void FocusNode(object sender, ExecutedRoutedEventArgs e)
        {
            double offsetX = 0.0;
            double offsetY = 0.0;

            var vm = (DataContext as GraphEditor);
            var dx = vm.PanOffset.X;
            var dy = vm.PanOffset.Y;

            if (vm.NodeSelection.Count > 0)
            {
                var nodeView = this.FindVisualChild<NodeView>();
                if (nodeView == null) return;

                var nodeCanvas = nodeView.FindVisualParent<Canvas>();
                if (nodeCanvas == null) return;

                double t = double.MaxValue, l = double.MaxValue, b = double.MinValue, r = double.MinValue;

                foreach (var child in nodeCanvas.Children)
                {
                    var node = child as FrameworkElement;
                    if ((node.DataContext as Node).IsSelected)
                    {
                        l = Math.Min(l, Canvas.GetLeft(node) + dx);
                        t = Math.Min(t, Canvas.GetTop(node) + dy);
                        r = Math.Max(r, Canvas.GetLeft(node) + dx + node.ActualWidth);
                        b = Math.Max(b, Canvas.GetTop(node) + dy + node.ActualHeight);
                    }
                }

                offsetX = ((RenderSize.Width / vm.ScaleFactor - (r - l)) * 0.5) - l;
                offsetY = ((RenderSize.Height / vm.ScaleFactor - (b - t)) * 0.5) - t;
            }
            else if (vm.GroupSelection.Count > 0)
            {
                var groupView = this.FindVisualChild<GroupNodeView>();
                if (groupView == null) return;

                var groupCanvas = groupView.FindVisualParent<Canvas>();
                if (groupCanvas == null) return;

                double t = double.MaxValue, l = double.MaxValue, b = double.MinValue, r = double.MinValue;

                foreach (var child in groupCanvas.Children)
                {
                    var group = child as FrameworkElement;
                    if ((group.DataContext as GroupNode).IsSelected)
                    {
                        l = Math.Min(l, Canvas.GetLeft(group) + dx);
                        t = Math.Min(t, Canvas.GetTop(group) + dy);
                        r = Math.Max(r, Canvas.GetLeft(group) + dx + group.ActualWidth);
                        b = Math.Max(b, Canvas.GetTop(group) + dy + group.ActualHeight);
                    }
                }

                offsetX = ((RenderSize.Width / vm.ScaleFactor - (r - l)) * 0.5) - l;
                offsetY = ((RenderSize.Height / vm.ScaleFactor - (b - t)) * 0.5) - t;
            }
            else if (vm.Nodes.Count > 0)
            {
                double minX = double.MaxValue;
                double minY = double.MaxValue;

                foreach (var node in vm.Nodes)
                {
                    double x = node.TopLeft.X + vm.PanOffset.X;
                    double y = node.TopLeft.Y + vm.PanOffset.Y;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                }
                foreach (var group in vm.Groups)
                {
                    double x = group.TopLeft.X + vm.PanOffset.X;
                    double y = group.TopLeft.Y + vm.PanOffset.Y;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                }


                offsetX = (30.0 / vm.ScaleFactor) - minX;
                offsetY = (30.0 / vm.ScaleFactor) - minY;
            }

            vm.PanOffset = new Point(vm.PanOffset.X + offsetX, vm.PanOffset.Y + offsetY);

            viewport.X += offsetX;
            viewport.Y += offsetY;
            bgImage.Viewport = viewport;
        }

        private void SelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            vm.NodeSelection.Clear();
            foreach (var node in vm.Nodes)
            {
                node.IsSelected = true;
                vm.NodeSelection.Add(node);
            }
        }

        private void GroupNodes(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (vm.NodeSelection.Count == 0 && vm.GroupSelection.Count == 0) return;

            foreach (var node in vm.NodeSelection)
            {
                if (node.ParentGroup != null && !vm.Groups.Contains(node.ParentGroup))
                    node.ParentGroup = null;
            }
            foreach (var group in vm.GroupSelection)
            {
                if (group.ParentGroup != null && !vm.Groups.Contains(group.ParentGroup))
                    group.ParentGroup = null;
            }

            var parentGroup = vm.NodeSelection.FirstOrDefault()?.ParentGroup;
            if (parentGroup == null) parentGroup = vm.GroupSelection.FirstOrDefault()?.ParentGroup;

            if ((vm.NodeSelection.FirstOrDefault(x => x.ParentGroup != parentGroup) != null) ||
                (vm.GroupSelection.FirstOrDefault(x => x.ParentGroup != parentGroup) != null))
                return;

            var newGroup = new GroupNode(vm.NodeSelection, vm.GroupSelection);
            vm.AddGroupNode(newGroup);
            parentGroup?.AddChild(newGroup);
        }

        private void UngroupNodes(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (vm.GroupSelection.Count == 0) return;

            foreach (var group in vm.GroupSelection)
            {
                var childNodes = group.ChildNodes.ToArray();
                foreach (var childNode in childNodes)
                {
                    group.RemoveChild(childNode);
                    group.ParentGroup?.AddChild(childNode);
                }

                var childGroups = group.ChildGroups.ToArray();
                foreach (var childGroup in childGroups)
                {
                    group.RemoveChild(childGroup);
                    group.ParentGroup?.AddChild(childGroup);
                }
                vm.RemoveGroupNode(group);
            }
            vm.GroupSelection.Clear();
        }

        private void UnselectGroupNodes(GraphEditor graphVM)
        {
            // Unselect any selected node
            foreach (var group in graphVM.GroupSelection)
            {
                group.IsSelected = false;
                group.IsMoving = false;
            }
            graphVM.GroupSelection.Clear();
        }

        private void UnselectNodes(GraphEditor graphVM)
        {
            // Unselect any selected node
            foreach (var node in graphVM.NodeSelection)
            {
                node.IsSelected = false;
            }
            graphVM.NodeSelection.Clear();
        }

        private void UnselectTransition(GraphEditor graphVM)
        {
            // Unselect any selected transition
            if (graphVM.SelectedTransition != null)
            {
                graphVM.SelectedTransition.IsSelected = false;
                graphVM.SelectedTransition = null;
            }
        }

        private double SnapToGrid(double d)
        {
            double gridSize = 10.0;
            return Math.Round(d / gridSize) * gridSize;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void SetGroupNodeSelection(GroupNode group)
        {
            var graphVM = (DataContext as GraphEditor);
            UnselectTransition(graphVM);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (group.IsSelected)
                {
                    group.IsSelected = false;
                    graphVM.GroupSelection.Remove(group);
                }
                else
                {
                    group.IsSelected = true;
                    graphVM.GroupSelection.Add(group);
                }
            }
            else // New selection
            {
                UnselectNodes(graphVM);
                UnselectGroupNodes(graphVM);
                group.IsSelected = true;
                graphVM.GroupSelection.Add(group);
            }
        }

        private void SetNodeSelection(Node node)
        {
            var graphVM = (DataContext as GraphEditor);
            UnselectTransition(graphVM);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (node.IsSelected)
                {
                    node.IsSelected = false;
                    graphVM.NodeSelection.Remove(node);
                }
                else
                {
                    node.IsSelected = true;
                    graphVM.NodeSelection.Add(node);
                }
            }
            else // New selection
            {
                UnselectNodes(graphVM);
                UnselectGroupNodes(graphVM);
                node.IsSelected = true;
                graphVM.NodeSelection.Add(node);
            }
        }

        private void SetTransitionSelection(Transition transition)
        {
            var vm = (DataContext as GraphEditor);

            UnselectNodes(vm);
            UnselectGroupNodes(vm);

            if (vm.SelectedTransition != transition)
            {
                transition.IsSelected = true;
                if (vm.SelectedTransition != null)
                {
                    vm.SelectedTransition.IsSelected = false;
                }
                vm.SelectedTransition = transition;
            }
        }
        private void Grid_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) return;

            boxTopLeft = e.GetPosition(this);

            boxSelectStarted = true;
            CaptureMouse(sender as IInputElement);
        }

        private void Grid_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            if (!boxSelectStarted) return;
            var p = e.GetPosition(this);

            var vm = DataContext as GraphEditor;
            var selection = GetBoxSelection(boxTopLeft, p);
            if (selection.Count > 0)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    foreach (var node in selection)
                    {
                        if (node.IsSelected)
                        {
                            node.IsSelected = false;
                            vm.NodeSelection.Remove(node);
                        }
                        else
                        {
                            node.IsSelected = true;
                            vm.NodeSelection.Add(node);
                        }
                    }
                }
                else // New selection
                {
                    foreach (var node in vm.NodeSelection)
                    {
                        node.IsSelected = false;
                    }
                    vm.NodeSelection.Clear();
                    foreach (var node in selection)
                    {
                        node.IsSelected = true;
                        vm.NodeSelection.Add(node);
                    }
                    UnselectGroupNodes(vm);
                }
            }
            else
            {
                UnselectNodes(vm);
                UnselectGroupNodes(vm);
            }

            UnselectTransition(vm);
            selectionBox.Visibility = Visibility.Collapsed;
            boxSelectStarted = false;
            CaptureMouse(null);
        }

        private List<Node> GetBoxSelection(Point topLeft, Point bottomRight)
        {
            List<Node> selection = new List<Node>();

            // Get a random node if any
            var nodeView = this.FindVisualChild<NodeView>();
            if (nodeView == null) return selection;
            // Get the containing canvas
            var canvas = nodeView.FindVisualParent<Canvas>();
            if (canvas == null) return selection;

            // Check if the nodes in canvas are completely within selection box
            var invScale = 1.0 / (DataContext as GraphEditor).ScaleFactor;
            var L = Math.Min(topLeft.X, bottomRight.X) * invScale;
            var T = Math.Min(topLeft.Y, bottomRight.Y) * invScale;
            var R = Math.Max(topLeft.X, bottomRight.X) * invScale;
            var B = Math.Max(topLeft.Y, bottomRight.Y) * invScale;

            var vm = DataContext as GraphEditor;
            var dx = vm.PanOffset.X;
            var dy = vm.PanOffset.Y;

            foreach (var child in canvas.Children)
            {
                var node = child as FrameworkElement;
                var left = Canvas.GetLeft(node) + dx;
                var top = Canvas.GetTop(node) + dy;
                var right = left + node.ActualWidth;
                var bottom = top + node.ActualHeight;

                if (left >= L && top >= T && right <= R && bottom <= B)
                {
                    Assert(node.DataContext is Node);
                    selection.Add(node.DataContext as Node);
                }
            }

            return selection;
        }

        private void Grid_Mouse_RBD(object sender, MouseButtonEventArgs e)
        {
            if (nodesMoving) return;
            gridClickPos = e.GetPosition(this);

            viewport.X = bgImage.Viewport.X;
            viewport.Y = bgImage.Viewport.Y;

            capturedRight = true;
            CaptureMouse(sender as IInputElement);
        }

        private void Grid_Mouse_RBU(object sender, MouseButtonEventArgs e)
        {
            capturedRight = false;
            CaptureMouse(null);
        }

        private void Grid_Mouse_Move(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            var vm = DataContext as GraphEditor;
            var invScale = 1.0 / vm.ScaleFactor;
            var mousePos = e.GetPosition(this);
            if (capturedRight && (grid != null))
            {
                var offset = mousePos - gridClickPos;

                boxTopLeft.X += offset.X;
                boxTopLeft.Y += offset.Y;

                offset *= invScale;

                viewport.X += offset.X;
                viewport.Y += offset.Y;

                bgImage.Viewport = viewport;

                vm.PanOffset = new Point(vm.PanOffset.X + offset.X, vm.PanOffset.Y + offset.Y);

                gridClickPos = mousePos;
            }

            if (boxSelectStarted && grid != null)
            {
                var d = boxTopLeft - mousePos;
                var x = Math.Min(mousePos.X, boxTopLeft.X) * invScale;
                var y = Math.Min(mousePos.Y, boxTopLeft.Y) * invScale;
                selectionBox.Width = Math.Abs(d.X) * invScale;
                selectionBox.Height = Math.Abs(d.Y) * invScale;
                Canvas.SetLeft(selectionBox, x);
                Canvas.SetTop(selectionBox, y);
                selectionBox.Visibility = Visibility.Visible;
            }
        }

        private void Grid_Mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            if (boxSelectStarted || capturedLeft) return;
            var vm = (DataContext as GraphEditor);
            if (zoomLabel.Opacity > 0)
            {
                var oldScale = vm.ScaleFactor;

                var newScaleFactor = vm.ScaleFactor + Math.Sign(e.Delta) * 0.1;
                bool scaleChanged = true;
                if (newScaleFactor - 0.1 <= -0.0001 || newScaleFactor - 1.5 >= 0.0001)
                {
                    scaleChanged = false;
                }

                if (scaleChanged)
                {
                    vm.ScaleFactor = newScaleFactor;

                    var mousePos = e.GetPosition(this);

                    var newPos = new Point(
                        mousePos.X * newScaleFactor / oldScale,
                        mousePos.Y * newScaleFactor / oldScale);

                    double offsetX = (mousePos.X - newPos.X) / newScaleFactor;
                    double offsetY = (mousePos.Y - newPos.Y) / newScaleFactor;

                    viewport.X += offsetX;
                    viewport.Y += offsetY;

                    bgImage.Viewport = viewport;

                    vm.PanOffset = new Point(vm.PanOffset.X + offsetX, vm.PanOffset.Y + offsetY);
                }
            }
            zoomLabel.Content = "Zoom " + vm.ScaleFactor.ToString("#0%");

            DoubleAnimation fadeIn =
                new DoubleAnimation(0.45, 0.5, new Duration(TimeSpan.FromSeconds(2.0)));
            fadeIn.Completed += (s, arg) =>
            {
                DoubleAnimation fadeOut =
                    new DoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(2.0)));
                zoomLabel.BeginAnimation(OpacityProperty, fadeOut);
            };
            zoomLabel.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void GroupNode_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (!(((FrameworkElement)sender).DataContext is GroupNode groupVM)) return;

            if (!vm.GroupSelection.Contains(groupVM) ||
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SetGroupNodeSelection(groupVM);
            }

            foreach (var group in vm.GroupSelection)
            {
                (group as GroupNode).IsMoving = true;
            }

            gridClickPos = e.GetPosition(this);
            nodesMoved = false;
            nodesMoving = true;

            UIElement source = (UIElement)sender;
            capturedLeft = true;
            CaptureMouse(source);
            e.Handled = true;
        }

        private void GroupNode_Mouse_Move(object sender, MouseEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (capturedLeft)
            {
                var p = e.GetPosition(this);
                var d = (p - gridClickPos);
                if (d.LengthSquared > 25 || nodesMoved)
                {
                    d /= vm.ScaleFactor;
                    nodesMoved = true;
                    foreach (var group in vm.GroupSelection)
                    {
                        group.TopLeft = new Point(group.TopLeft.X + d.X, group.TopLeft.Y + d.Y);
                    }
                    gridClickPos = p;
                }
            }
        }

        private void GroupNode_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GraphEditor;

            if (!nodesMoved && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var groupVM = ((FrameworkElement)sender).DataContext as GroupNode;
                SetGroupNodeSelection(groupVM);
            }

            foreach (var group in vm.GroupSelection)
            {
                (group as GroupNode).IsMoving = false;
            }

            nodesMoved = false;
            nodesMoving = false;
            capturedLeft = false;
            CaptureMouse(null);
        }

        private void Node_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            var nodeVM = ((FrameworkElement)sender).DataContext as Node;

            if (!vm.NodeSelection.Contains(nodeVM) ||
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SetNodeSelection(nodeVM);
            }

            var presenter = VisualHelper.FindVisualParent<ContentPresenter>((DependencyObject)sender);
            Panel.SetZIndex(presenter, 10000);

            gridClickPos = e.GetPosition(this);

            UIElement source = (UIElement)sender;
            capturedLeft = true;
            nodesMoving = true;
            CaptureMouse(source);
            nodesMoved = false;
            e.Handled = true;
        }

        private void Node_Mouse_Move(object sender, MouseEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (capturedLeft)
            {
                var p = e.GetPosition(this);
                var d = (p - gridClickPos);
                if (d.LengthSquared > 25 || nodesMoved)
                {
                    d /= vm.ScaleFactor;
                    nodesMoved = true;
                    foreach (var node in vm.NodeSelection)
                    {
                        node.TopLeft = new Point(node.TopLeft.X + d.X, node.TopLeft.Y + d.Y);
                    }
                    gridClickPos = p;
                }
            }
        }

        private void Node_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GraphEditor;
            if (!nodesMoved && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var nodeVM = ((FrameworkElement)sender).DataContext as Node;
                SetNodeSelection(nodeVM);
            }

            if (nodesMoved)
            {
                foreach (var node in vm.NodeSelection)
                {
                    node.TopLeft = new Point(SnapToGrid(node.TopLeft.X), SnapToGrid(node.TopLeft.Y));
                }
            }

            nodesMoved = false;

            var presenter = VisualHelper.FindVisualParent<ContentPresenter>((DependencyObject)sender);
            Panel.SetZIndex(presenter, 0);
            capturedLeft = false;
            nodesMoving = false;
            CaptureMouse(null);
        }

        private void Transition_Mouse_LDB(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is Transition transition)
            {
                SetTransitionSelection(transition);
            }
            e.Handled = true;

        }

        private void Transition_Mouse_Move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                ((FrameworkElement)e.OriginalSource).DataContext is Transition transition)
            {
                SetTransitionSelection(transition);
            }
        }
    }
}
