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

        public Image HighlightImage { get; set; }
        public string Name { get; set; }


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
            if (!HasChildren)
            {
                return new List<Node>();
            }

            var rootNodeUrl = $"{SmugMug.BaseUrl}{ChildNodesUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(rootNodeUrl);

            children = await ListFromJson(response);
            return Children;
        }

        private async Task<Image> GetHighlight()
        {
            if (!string.IsNullOrWhiteSpace(HighlightImageUri) && HighlightImage == null)
            {
                var response = await SmugMug.GetResponseForProtectedRequest($"{SmugMug.BaseUrl}{HighlightImageUri}");
                HighlightImage = await Image.FromJson(response);
            }
            return HighlightImage;
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

        public async static Task<Node> FromJson(string json)
        {
            var responseObj = JObject.Parse(json);
            var jObj = responseObj["Response"]["Node"];

            var node = await ConvertNode(jObj);

            return node;
        }

        private async static Task<Node> ConvertNode(JToken jNode)
        {
            var node = JsonConvert.DeserializeObject<Node>(jNode.ToString());

            var searchable = jNode["SmugSearchable"].ToString();
            var worldSearchable = jNode["WorldSearchable"].ToString();

            node.SmugSearchable = searchable.ToLower().Contains("inherit") ?
                Searchable.Inherit :
                (Searchable)Enum.Parse(typeof(Searchable), searchable);
            node.WorldSearchable = worldSearchable.ToLower().Contains("inherit") ?
                WorldSearchable.Inherit :
                (WorldSearchable)Enum.Parse(typeof(WorldSearchable), worldSearchable);

            var nodeUris = jNode["Uris"];
            if (nodeUris != null)
            {
                node.FolderByIdUri = UriAtPath(nodeUris, "FolderByID");
                node.ParentNodesUri = UriAtPath(nodeUris, "ParentNodes");
                node.UserUri = UriAtPath(nodeUris, "User");
                node.HighlightImageUri = UriAtPath(nodeUris, "HighlightImage");
                node.ChildNodesUri = UriAtPath(nodeUris, "ChildNodes");
                node.MoveNodesUri = UriAtPath(nodeUris, "MoveNodes");
                node.NodeGrantsUri = UriAtPath(nodeUris, "NodeGrants");
            }

            await node.GetChildren();
            await node.GetHighlight();

            return node;
        }

        private static string UriAtPath(JToken uris, string name)
        {
            var uri = string.Empty;

            if (uris != null)
            {
                var token = uris[name];
                if (token != null)
                {
                    var uriToken = token["Uri"];
                    if (uriToken != null)
                    {
                        uri = uriToken.ToString();
                    }
                }
            }

            return uri;
        }

        public async static Task<List<Node>> ListFromJson(string response)
        {
            var responseObj = JObject.Parse(response);
            var jObjs = responseObj["Response"]["Node"];

            var nodeTasks = jObjs.Select(n => ConvertNode(n));
            var nodes = await Task.WhenAll(nodeTasks);

            return nodes.ToList();
        }
    }
}
