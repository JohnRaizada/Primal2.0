using PrimalEditor.Content;
using PrimalEditor.Dependencies;
using PrimalEditor.GameDev;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();
            Loaded += OnWorldEditorViewLoaded;
        }

        private void OnWorldEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorViewLoaded;
            Focus();
            ((INotifyCollectionChanged)Project.UndoRedo.UndoList).CollectionChanged += (s, e) => Focus();
            //Docks.Children.Remove(LoggerView);
            GenerateVisualTree();
        }
        private void GenerateVisualTree()
        {
            Node<DockedElement> worldEditor = Docking.AddElement(WorldEditor, nameof(WorldEditor));
            Node<DockedElement> statusBar = Docking.MakeElement(StatusBar, nameof(StatusBar));
            worldEditor.AddChild(statusBar);
            Node<DockedElement> contentArea = Docking.MakeElement(ContentArea, nameof(ContentArea));
            worldEditor.AddChild(contentArea);
            Node<DockedElement> renderSurfacesWithDocks = Docking.MakeElement(RenderSurfacesWithDocks, nameof(RenderSurfacesWithDocks));
            contentArea.AddChild(renderSurfacesWithDocks);
            Node<DockedElement> projectLayoutViewWithGameEntityView = Docking.MakeElement(ProjectLayoutViewWithGameEntityView, nameof(ProjectLayoutViewWithGameEntityView));
            contentArea.AddChild(projectLayoutViewWithGameEntityView);
            Node<DockedElement> renderSurfaces = Docking.MakeElement(RenderSurfaces, nameof(RenderSurfaces));
            renderSurfacesWithDocks.AddChild(renderSurfaces);
            Node<DockedElement> renderSurface1 = Docking.MakeElement(RenderSurface1, nameof(RenderSurface1));
            renderSurfaces.AddChild(renderSurface1);
            Node<DockedElement> renderSurface2 = Docking.MakeElement(RenderSurface2, nameof(RenderSurface2));
            renderSurfaces.AddChild(renderSurface2);
            Node<DockedElement> renderSurface3 = Docking.MakeElement(RenderSurface3, nameof(RenderSurface3));
            renderSurfaces.AddChild(renderSurface3);
            Node<DockedElement> renderSurface4 = Docking.MakeElement(RenderSurface4, nameof(RenderSurface4));
            renderSurfaces.AddChild(renderSurface4);
            Node<DockedElement> docks = Docking.MakeElement(Docks, nameof(Docks));
            renderSurfacesWithDocks.AddChild(docks);
            Node<DockedElement> historyView = Docking.MakeElement(HistoryView, nameof(HistoryView));
            docks.AddChild(historyView);
            Node<DockedElement> loggerView = Docking.MakeElement(LoggerView, nameof(LoggerView));
            docks.AddChild(loggerView);
            Node<DockedElement> contentBrowserView = Docking.MakeElement(ContentBrowserView, nameof(ContentBrowserView));
            docks.AddChild(contentBrowserView);
            Node<DockedElement> projectLayoutView = Docking.MakeElement(ProjectLayoutView, nameof(ProjectLayoutView));
            projectLayoutViewWithGameEntityView.AddChild(projectLayoutView);
            Node<DockedElement> gameEntityView = Docking.MakeElement(GameEntityView, nameof(GameEntityView));
            projectLayoutViewWithGameEntityView.AddChild(gameEntityView);
        }
        private void Generate()
        {

        }
        private void OnNewScript_Button_Click(object sender, RoutedEventArgs e)
        {
            new NewScriptDialog().ShowDialog();
        }

        private void OnCreatePrimitiveMesh_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrimitiveMeshDialog();
            dlg.ShowDialog();
        }

        private void OnNewProject(object sender, ExecutedRoutedEventArgs e)
        {
            ProjectBrowserDialog.GotoNewProjectTab = true;
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void OnOpenProject(object sender, ExecutedRoutedEventArgs e)
        {
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void OnEditorClose(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void OnPlatform_Menuitem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnEngine_Menuitem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnTools_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;
            switch (menuItem.Header.ToString())
            {
                case "SDK Manager":
                    SDKManagerView dialog = new SDKManagerView();
                    dialog.ShowDialog();
                    break;
                default:
                    break;
            }
        }
    }
}
