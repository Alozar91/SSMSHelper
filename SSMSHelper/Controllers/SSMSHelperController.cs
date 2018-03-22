using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using SSMSHelper.MenuItems;

namespace SSMSHelper.Controllers
{
    public class SSMSHelperController
    {
        static Dictionary<string, SQLMenuItem> nodeMenus = new Dictionary<string, SQLMenuItem>();
        //static SQLMenuItem currentMenu;
        public readonly DTE application;
        public readonly SSMSHelperPackage package;
        public readonly IObjectExplorerService objectExplorer;

        public SSMSHelperController(DTE app, SSMSHelperPackage pack, IObjectExplorerService ObjectExplorer)
        {
            application = app;
            package = pack;
            objectExplorer = ObjectExplorer;
        }

        public void SetObjectExplorerEvents()
        {
            if (objectExplorer != null) {
                var oesTreeProperty = objectExplorer.GetType().GetProperty("Tree", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (oesTreeProperty != null) {
                    TreeView tv = (TreeView)oesTreeProperty.GetValue(objectExplorer, null);
                    //tv.ContextMenuChanged += new EventHandler(ObjectExplorerTreeViewAfterSelectCallback);
                    tv.AfterSelect += new TreeViewEventHandler(ObjectExplorerTreeViewAfterSelectCallback);

                }
            }
        }

        void ObjectExplorerTreeViewAfterSelectCallback(object sender, TreeViewEventArgs e)
        {            

            INodeInformation[] nodes;
            INodeInformation node;
            int nodeCount;

            objectExplorer.GetSelectedNodes(out nodeCount, out nodes);
            node = nodeCount > 0 ? nodes[0] : null;

            if (!nodeMenus.Keys.Any(f => f==node.UrnPath)) {
                var _tableMenu = (HierarchyObject)node.GetService(typeof(IMenuHandler));
         //       if (_tableMenu.Parent.Name == node.Name) {
                    SQLMenuItem item = new SQLMenuItem(objectExplorer, package);
                    _tableMenu.AddChild(string.Empty, item);
                    //if (currentMenu != null) {
                    //    currentMenu.Dispose();
                    //}
                    //currentMenu = item;
                    nodeMenus.Add(node.UrnPath, item);
            //    }
            }
            //var menu = (HierarchyObject)node.GetService(typeof(IMenuHandler));
            //var menu = e.Node.ContextMenu;
            //menu.MenuItems.Add(e.Node.FullPath + e.Node.Text);
            //menu.AddChild(string.Empty, mn);

        }

    }
}
