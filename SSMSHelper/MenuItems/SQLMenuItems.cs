using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using SSMSHelper.Options;

namespace SSMSHelper.MenuItems
{
    public class SQLMenuItem : ToolsMenuItemBase, IWinformsMenuHandler, IDisposable
    {
        private INodeInformation nodeInformation;
        private IObjectExplorerService objectExplorer;
        private SSMSHelperPackage package;

        private const string ContextRegEx = @"^(Server\[@Name='(?<Server>[^']*)'\])*(\/Database\[@Name='(?<Database>[^']*)'\])*(\/Table\[@Name='(?<Table>[^']*)' and @Schema='(?<Schema>[^']*)'\])*(\/Column\[@Name='(?<Column>[^']*)'\])*$";

        public SQLMenuItem(INodeInformation Node, SSMSHelperPackage Package)
        {
            nodeInformation = Node;
            package = Package;
        }

        public SQLMenuItem(IObjectExplorerService objExp, SSMSHelperPackage Package)
        {
            objectExplorer = objExp;
            package = Package;
        }

        #region Override Methods
        /// <summary>
        /// Invokes this instance.
        /// </summary>
        protected override void Invoke()
        {
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new SQLMenuItem(nodeInformation, package);
        }
        #endregion

        #region IWinformsMenuHandler Members
        /// <summary>
        /// Gets the menu items.
        /// </summary>
        /// <returns></returns>
        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            /*context menu*/
            ToolStripMenuItem item = new ToolStripMenuItem("Шаблоны запросов");
            INodeInformation[] nodes;
            INodeInformation node;
            int nodeCount;

            objectExplorer.GetSelectedNodes(out nodeCount, out nodes);
            node = nodeCount > 0 ? nodes[0] : null;
            //QueryTemplate[] templates = GetTemplates(nodeInformation.UrnPath, nodeInformation.Name);
            QueryTemplate[] templates = GetTemplates(node.UrnPath, node.Name);
            foreach (var template in templates) {
                ToolStripMenuItem nm = new ToolStripMenuItem(template.Name);
                nm.Click += new EventHandler(ScriptQuery_Click);
                nm.Tag = template;
                item.DropDownItems.Add(nm);
            }

            //ToolStripMenuItem nm = new ToolStripMenuItem(nodeInformation.UrnPath + "////"+ nodeInformation.InvariantName + "////" + nodeInformation.Name);
            //nm.Click += new EventHandler(ScriptQuery_Click);
            ///*context submenu item - generate inserts*/
            //item.DropDownItems.Add(nm);

            return new ToolStripItem[] { item };
        }
        #endregion

        #region Events Methods
        private void ScriptQuery_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            QueryTemplate template = (QueryTemplate)item.Tag;
            string context = this.Parent.Context;
            string[] text = GetQueryByContext(((QueryTemplate)item.Tag).Template, context);
            ServiceCache.ScriptFactory.CreateNewBlankScript(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptType.Sql);
            EnvDTE.TextDocument doc = (EnvDTE.TextDocument)ServiceCache.ExtensibilityModel.ActiveDocument.Object(null);
            StringBuilder sb = new StringBuilder();
            foreach (string str in text) {
                sb.AppendLine(str);
            }
            doc.EndPoint.CreateEditPoint().Insert(sb.ToString());
            if (template.Autoexec) {
                doc.DTE.ExecuteCommand("Query.Execute");
            }

        }
        #endregion

        #region Helpers Methods
        private QueryTemplate[] GetTemplates(string nodeUrnPath, string nodeName)
        {
            QueryTemplate[] filteredTemplates = new QueryTemplate[0];
            GeneralOptions options = (GeneralOptions)package.GetDialogPage(typeof(GeneralOptions));
            QueryTemplate[] templates = GetTemplatesByNodeUrnPath(nodeUrnPath);
            if (templates != null) {
                filteredTemplates= templates.Where(t => t.Objects == "*" || t.Objects.Contains(nodeName)).ToArray();
            }
            return filteredTemplates;
            //foreach (QueryTemplate template in templates) {
            //    if (template.Objects == "*" || template.Objects.Contains(nodeName)) {

            //    }
            //}
        }

        private string[] GetQueryByContext(string[] template, string context)
        {
            string[] query = template;
            Regex reg = new Regex(ContextRegEx);
            Match m = reg.Match(context);
            if (!m.Success) {
                throw new ArgumentException("Контекст не распознан");
            }
            var serverGroup = m.Groups["Server"];
            var databaseGroup = m.Groups["Database"];
            var schemaGroup = m.Groups["Schema"];
            var tableGroup = m.Groups["Table"];
            var columnGroup = m.Groups["Column"];
            for (int i = 0; i < query.Length; i++) {
                if (serverGroup.Success) {
                    query[i] = query[i].Replace("{SERVER}", serverGroup.Value);
                }
                if (databaseGroup.Success) {
                    query[i] = query[i].Replace("{DATABASE}", databaseGroup.Value);
                }
                if (schemaGroup.Success) {
                    query[i] = query[i].Replace("{SCHEMA}", schemaGroup.Value);
                }
                if (tableGroup.Success) {
                    query[i] = query[i].Replace("{TABLE}", tableGroup.Value);
                }
                if (columnGroup.Success) {
                    query[i] = query[i].Replace("{COLUMN}", columnGroup.Value);
                }
            }
            return query;
        }
        private QueryTemplate[] GetTemplatesByNodeUrnPath(string urnPath)
        {
            QueryTemplate[] templates = new QueryTemplate[0];
            GeneralOptions options = (GeneralOptions)package.GetDialogPage(typeof(GeneralOptions));
            switch (urnPath) {
                case "Server":
                    templates = options.ServerTemplates;
                    break;
                case "Server/Database":
                    templates = options.DatabaseTemplates;
                    break;
                case "Server/Database/Table":
                    templates = options.TableTemplates;
                    break;
                case "Server/Database/Table/Column":
                    templates = options.ColumnTemplates;
                    break;               
            }
            return templates;
        }

        private Dictionary<string,string> GetNodeContextArray(string context)
        {
            Dictionary<string, string> contextArray = new Dictionary<string, string>();
            var regex = new Regex(ContextRegEx);
            Match match = regex.Match(context);
            if (match != null) {
                contextArray.Add("Server", match.Groups["Server"].Value);
                contextArray.Add("Database", match.Groups["Database"].Value);
                contextArray.Add("Table", match.Groups["Table"].Value);
                contextArray.Add("Column", match.Groups["Column"].Value);
            }
            return contextArray;
        }

        public void Dispose()
        {
            Dispose(true /*called by user directly*/);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            return;
            //if (disposing) {
            //    // Освобождаем только управляемые ресурсы
            //}

            //// Освобождаем неуправляемые ресурсы
        }
        #endregion

        //private void Count_Click(object sender, EventArgs e)
        //{
        //    ToolStripMenuItem item = (ToolStripMenuItem)sender;

        //    ServiceCache.ScriptFactory.CreateNewBlankScript(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptType.Sql);
        //    EnvDTE.TextDocument doc = (EnvDTE.TextDocument)ServiceCache.ExtensibilityModel.ActiveDocument.Object(null);
        //    doc.EndPoint.CreateEditPoint().Insert(this.Parent.Connection.ConnectionString + "-----" + this.Parent.Context);
        //    for (int i = 0; i < 10; i++) {
        //        DebugWindow window = (DebugWindow)package.FindToolWindow(typeof(DebugWindow), i, false);
        //        if (window != null) {
        //            DebugWindowControl ctrl = (DebugWindowControl)window.Content;
        //            ctrl.AddItem(this.Parent.Connection.ConnectionString + "-----" + this.Parent.Context);
        //            break;
        //        }
        //    }
        //    //bool generateColumnNames = (bool)item.Tag;

        //    //Match match = this.tableRegex.Match(this.Parent.Context);
        //    //if (match != null)
        //    //{
        //    //    string tableName = match.Groups["Table"].Value;
        //    //    string schema = match.Groups["Schema"].Value;
        //    //    string database = match.Groups["Database"].Value;
        //    //    string connectionString = this.Parent.Connection.ConnectionString + ";Database=" + database;
        //    //    string sqlStatement = string.Format(SSMSAddin.Properties.Resources.SQLCount, schema, tableName);

        //    //    SqlCommand command = new SqlCommand(sqlStatement);
        //    //    command.Connection = new SqlConnection(connectionString);
        //    //    command.Connection.Open();
        //    //    int tableCount = int.Parse(command.ExecuteScalar().ToString());
        //    //    command.Connection.Close();

        //    //    StringBuilder resultCaption = new StringBuilder().AppendFormat("{0} /*{1:n0}*/", sqlStatement, tableCount);

        //    //    this.dteController.CreateNewScriptWindow(resultCaption); // create new document
        //    //    this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query
        //    //}
        //}
    }
}
