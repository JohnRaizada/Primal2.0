using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace PrimalEditor.Utilities
{
    class Docking : ViewModelBase
    {
        private static Node<DockedElement> _visualTree;
        private static List<(Node<DockedElement> Parent, Node<DockedElement> Node)> _removedNodes = new List<(Node<DockedElement> Parent, Node<DockedElement> Node)>();
        private static Dictionary<string, Window> _windows = new Dictionary<string, Window>();
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WM_MOVING = 0x0216;

        internal static Node<DockedElement> AddElement(UIElement uIElement, string name, bool isDocked = true)
        {
            DockedElement dockedElement = new DockedElement(uIElement, name, isDocked);
            Node<DockedElement> node = new Node<DockedElement>(dockedElement);
            if (_visualTree == null)
            {
                _visualTree = node;
                return _visualTree;
            }
            _visualTree.AddChild(node);
            return _visualTree;
        }
        internal static Node<DockedElement> MakeElement(UIElement uIElement, string name, bool isDocked = true)
        {
            DockedElement dockedElement = new DockedElement(uIElement, name, isDocked);
            Node<DockedElement> node = new Node<DockedElement>(dockedElement);
            return node;
        }
        internal static void RemoveElement(string name)
        {
            // Find the element with the given name in the _visualTree
            Node<DockedElement> element = _visualTree.Find(x => x.Name == name);
            if (element == null) return;
            // Find the parent of the element in the _visualTree
            Node<DockedElement> parent = _visualTree.FindParent(element);
            if (!(parent.Value.Element is Panel panel)) return;

            // Remove the element from its parent panel
            UIElement elementToRemove = element.Value.Element;
            GetElementCornerCoordinates(elementToRemove);
            panel.Children.Remove(elementToRemove);

            // Add the element and its parent to the _removedNodes list before removing it from the _visualTree
            _removedNodes.Add((parent, element));
            parent.Children.Remove(element);

            if (!(panel is Grid grid)) return;

            // Remove the column and row definitions if necessary
            int columnIndex = Grid.GetColumn(elementToRemove);
            if (grid.ColumnDefinitions.Count > columnIndex && IsColumnEmpty(grid, columnIndex))
                grid.ColumnDefinitions.RemoveAt(columnIndex);

            int rowIndex = Grid.GetRow(elementToRemove);
            if (grid.RowDefinitions.Count > rowIndex && IsRowEmpty(grid, rowIndex))
                grid.RowDefinitions.RemoveAt(rowIndex);
        }
        internal static void AddElementBack(string name)
        {
            // Find the element with the given name in the _removedNodes list
            var removedNode = _removedNodes.Find(x => x.Node.Value.Name == name);
            if (removedNode == default) return;

            // Get the parent and element from the removedNode
            var parent = removedNode.Parent;
            var element = removedNode.Node;

            // Get the column and row index of the element
            int columnIndex = Grid.GetColumn(element.Value.Element);
            int rowIndex = Grid.GetRow(element.Value.Element);

            // Add the element back to its parent panel using the AddElementAt method
            AddElementAt(parent.Value.Name, element.Value.Name, rowIndex, columnIndex);

            // Remove the element from the _removedNodes list
            _removedNodes.Remove(removedNode);
        }
        internal static void AddElementAt(string parentName, string elementName, int? rowIndex = null, int? columnIndex = null)
        {
            Node<DockedElement> parent = _visualTree.Find(x => x.Name == parentName);
            if (!(parent?.Value.Element is Grid grid)) return;
            if (rowIndex.HasValue && rowIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            if (columnIndex.HasValue && columnIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            var removedNode = _removedNodes.FirstOrDefault(x => x.Node.Value.Name == elementName);
            if (removedNode == default) return;


            Node<DockedElement> element = removedNode.Node;
            RemoveGridSplitters(grid);
            if (rowIndex.HasValue && columnIndex.HasValue)
                try
                {
                    foreach (UIElement child in grid.Children)
                    {
                        if (!IsLengthJustified(grid, grid.RowDefinitions.Count - 1 >= 0 ? grid.RowDefinitions.Count - 1 : grid.RowDefinitions.Count, grid.ColumnDefinitions.Count - 1 >= 0 ? grid.ColumnDefinitions.Count - 1 : grid.ColumnDefinitions.Count) || ((Grid.GetRow(child) == rowIndex) && (Grid.GetColumn(child) == columnIndex)) && !IsGridSplitter(child))
                        {
                            Rearrange(grid, rowIndex.Value, columnIndex.Value, element, parentName);
                        }
                    }
                }
                catch (InvalidOperationException)
                {

                }

            parent.Children.Add(element);


            _removedNodes.Remove(removedNode);


            UIElement elementToAdd = element.Value.Element;
            grid.Children.Add(elementToAdd);


            if (rowIndex.HasValue)
                Grid.SetRow(elementToAdd, rowIndex.Value);
            if (columnIndex.HasValue)
                Grid.SetColumn(elementToAdd, columnIndex.Value);
            while (columnIndex.HasValue && grid.ColumnDefinitions.Count <= columnIndex)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            while (rowIndex.HasValue && grid.RowDefinitions.Count <= rowIndex)
                grid.RowDefinitions.Add(new RowDefinition());
            RefreshGridSplitters(grid);
        }

        private static void RefreshGridSplitters(Grid grid)
        {
            RemoveGridSplitters(grid);
            AddGridSplitters(grid);
        }

        private static void AddGridSplitters(Grid grid)
        {
            int i = 0;
            int j = 0;
            int columnCount = grid.ColumnDefinitions.Count;
            int rowCount = grid.RowDefinitions.Count;
            while (j < rowCount - 1)
            {
                AddGridSplitter(grid, j, 0, 1, columnCount, VerticalAlignment.Bottom, HorizontalAlignment.Stretch, height: 5);
                j++;
            }
            while (i < columnCount - 1)
            {
                AddGridSplitter(grid, 0, i, rowCount, 1, VerticalAlignment.Stretch, HorizontalAlignment.Right, width: 5);
                i++;
            }
        }

        private static void AddGridSplitter(Grid grid, int rowIndex, int columnIndex, int rowSpan, int columnSpan, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, double height = double.NaN, double width = double.NaN)
        {
            GridSplitter splitter = new GridSplitter()
            {
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                Background = new SolidColorBrush(Colors.Red),
                Height = height,
                Width = width
            };
            grid.Children.Add(splitter);
            Grid.SetRow(splitter, rowIndex);
            Grid.SetColumn(splitter, columnIndex);
            Grid.SetRowSpan(splitter, rowSpan);
            Grid.SetColumnSpan(splitter, columnSpan);
        }
        private static void RemoveGridSplitters(Grid grid)
        {
            var gridSplitters = grid.Children.OfType<GridSplitter>().ToList();
            foreach (var splitter in gridSplitters)
            {
                grid.Children.Remove(splitter);
            }
        }
        private static bool IsGridSplitter(UIElement element)
        {
            return element is GridSplitter;
        }
        private static void Rearrange(Grid grid, int row, int column, Node<DockedElement> element, string parentName)
        {
            // Check if the target element is already placed. If this is the case return
            if (GetElementAt(grid, row, column) == element) return;
            if (grid.ColumnDefinitions.Count > 1)
            {
                // If the grid has columns defined, make space by moving columns
                int columnLength = grid.ColumnDefinitions.Count;
                if (!IsLengthJustified(grid, row, grid.ColumnDefinitions.Count - 1) || !IsCellEmpty(grid, row, grid.ColumnDefinitions.Count - 1)) grid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int i = columnLength - column - 1; i >= column; i--)
                {
                    Node<DockedElement> elementToBeMoved = GetElementAt(grid, row, i);
                    if (elementToBeMoved == null) continue;
                    RemoveElement(elementToBeMoved.Value.Name);
                    AddElementAt(parentName, elementToBeMoved.Value.Name, row, i + 1);
                }
                //AddElementAt(parentName, element.Value.Name, row, column);
            }
            else if (grid.RowDefinitions.Count > 1)
            {
                // If the grid does not have columns defined but have rows defined, make space by moving rows
                if (!IsLengthJustified(grid, grid.RowDefinitions.Count - 1, column) || !IsCellEmpty(grid, grid.RowDefinitions.Count - 1, column)) grid.RowDefinitions.Add(new RowDefinition());
                for (int i = grid.RowDefinitions.Count - row - 1; i >= row; i--)
                {
                    Node<DockedElement> elementToBeMoved = GetElementAt(grid, i, column);
                    if (elementToBeMoved == null) continue;
                    RemoveElement(elementToBeMoved.Value.Name);
                    AddElementAt(parentName, elementToBeMoved.Value.Name, i + 1, column);
                }
            }
            else
            {
                // If the grid does not have either
            }
        }
        private static Node<DockedElement> GetElementAt(Grid grid, int row, int column)
        {
            Node<DockedElement> found = null;
            foreach (UIElement element in grid.Children)
            {
                if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                {
                    Node<DockedElement> target = _visualTree.Find(x => x.Element == element);
                    if (target == null) continue;
                    if (IsGridSplitter(element)) continue;
                    found = target;
                }
            }
            return found;
        }
        private static List<UIElement> GetColumnElements(Grid grid, int columnIndex)
        {
            List<UIElement> columnElements = new List<UIElement>();
            foreach (UIElement element in grid.Children)
                if (Grid.GetColumn(element) == columnIndex && !(element is GridSplitter))
                    columnElements.Add(element);
            return columnElements;
        }
        private static List<UIElement> GetRowElements(Grid grid, int rowIndex)
        {
            List<UIElement> rowElements = new List<UIElement>();
            foreach (UIElement element in grid.Children)
                if (Grid.GetRow(element) == rowIndex && !(element is GridSplitter))
                    rowElements.Add(element);
            return rowElements;
        }
        private static void ShiftColumnElementsjj(Grid grid, int columnIndex)
        {
            foreach (UIElement element in grid.Children)
                if (Grid.GetColumn(element) == columnIndex && !(element is GridSplitter))
                    Grid.SetColumn(element, columnIndex + 1);
        }
        private static void ShiftColumnElements(Grid grid, int columnIndex)
        {
            foreach (UIElement element in GetColumnElements(grid, columnIndex))
            {
                Grid.SetColumn(element, columnIndex + 1);
            }
        }
        private static void ShiftRowElements(Grid grid, int rowIndex)
        {
            foreach (UIElement element in grid.Children)
                if (Grid.GetRow(element) == rowIndex && !(element is GridSplitter))
                    Grid.SetRow(element, rowIndex + 1);
        }
        private static bool IsColumnEmpty(Grid grid, int columnIndex)
        {
            foreach (UIElement element in grid.Children)
                if (Grid.GetColumn(element) == columnIndex && !(element is GridSplitter))
                    return false;
            return true;
        }
        private static bool IsCellEmpty(Grid grid, int row, int column)
        {
            foreach (UIElement element in grid.Children)
            {
                if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool IsLengthJustified(Grid grid, int rowIndex, int columnIndex)
        {
            int highestRowIndex = 0;
            int highestColumnIndex = 0;
            bool justified = false;
            foreach (UIElement element in grid.Children)
            {
                if (Grid.GetRow(element) > highestRowIndex) highestRowIndex = Grid.GetRow(element);
                if (Grid.GetColumn(element) > highestColumnIndex) highestColumnIndex = Grid.GetColumn(element);
            }
            justified = rowIndex >= highestRowIndex && columnIndex >= highestColumnIndex;
            return justified;
        }
        private static bool IsRowEmpty(Grid grid, int rowIndex)
        {
            foreach (UIElement element in grid.Children)
                if (Grid.GetRow(element) == rowIndex && !(element is GridSplitter))
                    return false;
            return true;
        }
        private static void CreateWindowWithElement(UIElement element, string name)
        {
            // Create a new window
            Window window = new Window();

            // Set the content of the window to the specified element
            window.Content = element;
            //GetElementCornerCoordinates(element);
            // Set the margin and padding of the window to 0
            window.Margin = new Thickness(0);
            window.Padding = new Thickness(0);

            // If the element is a FrameworkElement, set its margin to 0
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.Margin = new Thickness(0);
            }

            // Apply the style to the window
            window.Style = (Style)window.FindResource("PrimalWindowStyle");
            _windows.Add(name, window);

            // Add a hook to the window procedure
            window.SourceInitialized += (s, e) =>
            {
                HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
                source.AddHook(WndProc);
            };

            // Show the window
            window.Show();
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ENTERSIZEMOVE:
                    Console.WriteLine("Dragged");
                    break;
                case WM_EXITSIZEMOVE:
                    break;
                case WM_MOVING:
                    Window window = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                    Console.WriteLine($"Current position: ({window.Left}, {window.Top})");
                    break;
            }
            return IntPtr.Zero;
        }
        private static Point[] GetElementCornerCoordinates(UIElement element)
        {
            // Get the position of the top-left corner of the element relative to the screen
            Point topLeft = element.PointToScreen(new Point(0, 0));

            // Get the position of the top-right corner of the element relative to the screen
            Point topRight = element.PointToScreen(new Point(element.RenderSize.Width, 0));

            // Get the position of the bottom-left corner of the element relative to the screen
            Point bottomLeft = element.PointToScreen(new Point(0, element.RenderSize.Height));

            // Get the position of the bottom-right corner of the element relative to the screen
            Point bottomRight = element.PointToScreen(new Point(element.RenderSize.Width, element.RenderSize.Height));

            return new Point[] { topLeft, topRight, bottomLeft, bottomRight };
        }
        internal static void CreateFloatingWindow(string name)
        {
            if (WindowExists(name)) return;
            Node<DockedElement> element = _visualTree.Find(x => x.Name == name);
            element.Value.IsDocked = false;
            RemoveElement(name);
            CreateWindowWithElement(element.Value.Element, element.Value.Name);
        }

        private static bool WindowExists(string name)
        {
            foreach (KeyValuePair<string, Window> window in _windows)
            {
                if (window.Key == name) return true;
            }
            return false;
        }

        private static void CloseFloatingWindow(string name)
        {
            foreach (KeyValuePair<string, Window> window in _windows)
            {
                if (window.Key != name) continue;
                window.Value.Close();
                _windows.Remove(window.Key);
            }
        }
        internal static void CloseAllWindows()
        {
            foreach (KeyValuePair<string, Window> window in _windows)
            {
                if (window.Value == null) continue;
                window.Value.Close();
                _windows.Remove(window.Key);
            }
        }

        internal static void CloseWindow(string name)
        {
            Node<DockedElement> element = _visualTree.Find(x => x.Name == name);
            if (element.Value.IsDocked)
            {
                RemoveElement(name);
                return;
            }
            CloseFloatingWindow(name);
            _windows.Remove(name);
        }

        internal static void DockWindow(string name)
        {
            if (!WindowExists(name)) return;
            var removedNode = _removedNodes.Find(x => x.Node.Value.Name == name);
            if (removedNode.Node.Value.IsDocked) return;
            removedNode.Node.Value.IsDocked = true;
            foreach (KeyValuePair<string, Window> window in _windows)
            {
                if (window.Key != name) continue;
                window.Value.Content = null;
                CloseFloatingWindow(name);
                AddElementBack(name);
                _windows.Remove(name);
            }
        }
        internal static bool GetIsDockEnabled(string name)
        {
            Node<DockedElement> element = _visualTree.Find(x => x.Name == name);
            return element.Value.IsDocked;
        }
    }
    sealed class DockedElement : ViewModelBase
    {
        internal UIElement Element { get; }
        internal string Name { get; }
        private bool _isDocked = true;
        internal bool IsDocked
        {
            get => _isDocked;
            set
            {
                if (_isDocked != value)
                {
                    _isDocked = value;
                    OnPropertyChanged(nameof(IsDocked));
                }
            }
        }
        internal DockedElement(UIElement element, string name, bool isDocked)
        {
            Element = element;
            Name = name;
            IsDocked = isDocked;
        }
    }
}
