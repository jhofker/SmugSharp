using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmugSharp.Models
{
    public enum Privacy { Public, Unlisted, Private }
    public enum SecurityType { None, Password, GrantAccess }
    public enum Searchable { No, Local, LocalUser, Yes, Inherit }
    public enum SortDirection { Ascending, Descending }
    public enum SortMethod { SortIndex, Name, DateAdded, DateModified }
    public enum NodeType { Album, Page, Folder }
    public enum WorldSearchable { No, HomeOnly, Yes, Inherit }

    public class Node
    {
        public DateTime DateAdded { get; set; }

        public DateTime DateModified { get; set; }

        /// <summary>
        /// Human-readable description for this node. 
        /// May contain basic HTML. Some node types display this to the user; 
        /// some merely use it for search engine optimization. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Effective privacy level of this node. 
        /// This takes into account the effect of privacy settings from this node's ancestors. 
        /// </summary>
        public Privacy EffectivePrivacy { get; set; }

        /// <summary>
        /// Effective security type of this node. 
        /// This takes into account the effect of security settings from this node's ancestors. 
        /// </summary>
        public SecurityType EffectiveSecurityType { get; set; }

        /// <summary>
        /// Whether this node contains any child nodes. 
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Whether to hide the identity of this node's owner from visitors. 
        /// Only applicable to Album nodes. 
        /// </summary>
        public bool HideOwner { get; set; }

        /// <summary>
        /// Whether this node is the owner\'s root node. 
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// Privacy level for this node. 
        /// Note: This may be overridden by a more restrictive privacy setting inherited 
        /// from one of this node's ancestors. See EffectivePrivacy. 
        /// </summary>
        public Privacy Privacy { get; set; }

        /// <summary>
        /// Security type for this node. 
        /// Note: This may be overridden by a more restrictive security setting inherited 
        /// from one of this node's ancestors. See EffectiveSecurityType. 
        /// </summary>
        public SecurityType SecurityType { get; set; }

        /// <summary>
        /// Acceptable values differ for root nodes and child nodes. 
        /// Root nodes: No, Local, LocalUser, Yes 
        /// Child nodes: No, Inherit from User 
        /// </summary>
        [JsonIgnoreAttribute]
        public Searchable SmugSearchable { get; set; }

        public SortDirection SortDirection { get; set; }

        public int SortIndex { get; set; }

        public SortMethod SortMethod { get; set; }

        public NodeType Type { get; set; }

        /// <summary>
        /// This is usually a URL-friendly version of the human-readable name. 
        /// Must start with a capital letter. 
        /// </summary>
        public string UrlName { get; set; }

        public string UrlPath { get; set; }

        /// <summary>
        /// Acceptable values differ for root nodes and child nodes. 
        /// Root nodes: No, HomeOnly, Yes 
        /// Child nodes: No, Inherit from User 
        /// </summary>
        [JsonIgnoreAttribute]
        public WorldSearchable WorldSearchable { get; set; }

        private List<Node> children;
        public List<Node> Children
        {
            get
            {
                if (HasChildren && children == null)
                {
                    children = GetChildren().Result;
                }
                return children;
            }
        }

        private async Task<List<Node>> GetChildren()
        {
            var rootNodeUrl = $"{SmugMug.BaseUrl}{ChildNodesUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(rootNodeUrl);

            return Node.ListFromJson(response);
        }

        #region Private Uris for fetching additional information
        private string FolderByIdUri { get; set; }
        private string ParentNodesUri { get; set; }
        private string UserUri { get; set; }
        private string HighlightImageUri { get; set; }
        private string ChildNodesUri { get; set; }
        private string MoveNodesUri { get; set; }
        private string NodeGrantsUri { get; set; }
        #endregion

        public static Node FromJson(string json, bool isFullResponse = true)
        {
            var responseObj = JObject.Parse(json);
            var jObj = responseObj["Response"]["Node"];

            var node = ConvertNode(jObj);

            return node;
        }

        private static Node ConvertNode(JToken jNode)
        {
            var node = JsonConvert.DeserializeObject<Node>(jNode.ToString());

            var searchable = jNode["SmugSearchable"].ToString();
            var worldSearchable = jNode["WorldSearchable"].ToString();

            node.SmugSearchable = searchable.ToLower().Contains("inherit") ?
                Searchable.Inherit :
                (Searchable)Enum.Parse(typeof(Searchable), searchable);
            node.WorldSearchable = searchable.ToLower().Contains("inherit") ?
                WorldSearchable.Inherit :
                (WorldSearchable)Enum.Parse(typeof(WorldSearchable), worldSearchable);

            var nodeUris = jNode["Uris"];
            if (nodeUris != null)
            {
                node.FolderByIdUri = nodeUris["FolderByID"]["Uri"].ToString();
                node.ParentNodesUri = nodeUris["ParentNodes"]["Uri"].ToString();
                node.UserUri = nodeUris["User"]["Uri"].ToString();
                node.HighlightImageUri = nodeUris["HighlightImage"]["Uri"].ToString();
                node.ChildNodesUri = nodeUris["ChildNodes"]["Uri"].ToString();
                node.MoveNodesUri = nodeUris["MoveNodes"]["Uri"].ToString();
                node.NodeGrantsUri = nodeUris["NodeGrants"]["Uri"].ToString();
            }

            return node;
        }

        public static List<Node> ListFromJson(string response)
        {
            var responseObj = JObject.Parse(response);
            var jObjs = responseObj["Response"]["Node"];

            var nodes = jObjs.Select(n => ConvertNode(n)).ToList();

            return nodes;
        }
    }
}
