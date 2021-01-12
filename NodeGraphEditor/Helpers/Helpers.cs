// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NodeGraphEditor.Helpers
{
    public static class VisualHelper
    {
        public static T FindVisualParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (!(obj is Visual)) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T type)
                {
                    return type;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        public static T FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (!(depObj is Visual)) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }

    public static class TypeConversion
    {
        public static bool CanConvert(Type source, Type target)
        {
            if (!source.IsAssignableFrom(target))
            {
                try
                {
                    var instanceOfSourceType = Activator.CreateInstance(source);
                    Convert.ChangeType(instanceOfSourceType, target);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static object ConvertTo(object value, Type target)
        {
            if (!value.GetType().IsAssignableFrom(target))
            {
                try
                {
                    return Convert.ChangeType(value, target);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
            return value;
        }
    }

    public static class MouseHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public POINT(Point pt) : this((int)pt.X, (int)pt.Y) { }

            public static implicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT((int)p.X, (int)p.Y);
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("User32.Dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        public static void SetCursor(IntPtr hWnd, int x, int y)
        {
            var p = new POINT(x, y);
            ClientToScreen(hWnd, ref p);
            SetCursorPos(p.X, p.Y);
        }

        public static Point GetCursor()
        {
            GetCursorPos(out POINT p);
            return p;
        }

    }
}
