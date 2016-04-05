using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmugSharp.Models
{
    public enum AccountStatus { Active, PastDue, Suspended, Closed }
    public enum SortBy { LastUpdated, Position }

    /// <summary>
    /// 
    /// Documentation: https://api.smugmug.com/api/v2/doc/reference/user.html
    /// </summary>
    public class User
    {
        /// <summary>
        /// Uri to fetch this object again
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Uri to this user's website
        /// </summary>
        public string WebUri { get; set; }

        /// <summary>
        /// User's nickname; generally [nickname].smugmug.com
        /// Note: Max length of 25 characters.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Current status of the user's account.
        /// </summary>
        public AccountStatus AccountStatus { get; set; }

        /// <summary>
        /// User's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's first name.
        /// Note: Max length of 20 characters.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// Note: Max length of 20 characters.
        /// </summary>
        public string LastName { get; set; }

        public bool FriendsView { get; set; }

        /// <summary>
        /// Total files uploaded by this user.
        /// </summary>
        public int ImageCount { get; set; }

        public bool IsTrial { get; set; }

        public SortBy SortBy { get; set; }

        public string ViewPassHint { get; set; }

        public string ViewPassword { get; set; }

        /// <summary>
        /// Same as <see cref="WebUri"/> without the protocol.
        /// </summary>
        public string Domain { get; set; }

        public string DomainOnly { get; set; }

        public string RefTag { get; set; }

        public string Plan { get; set; }

        #region Private Uris for fetching additional information
        private string BioImageUri { get; set; }
        private string CoverImageUri { get; set; }
        private string UserProfileUri { get; set; }
        private string NodeUri { get; set; }
        private string FolderUri { get; set; }
        private string UserAlbumsUri { get; set; }
        private string UserGeoMediaUri { get; set; }
        private string UserPopularMediaUri { get; set; }
        private string UserFeaturedAlbumsUri { get; set; }
        private string UserRecentImagesUri { get; set; }
        private string UserImageSearchUri { get; set; }
        private string UserTopKeywordsUri { get; set; }
        private string UrlPathLookupUri { get; set; }
        private string UserAlbumTemplatesUri { get; set; }
        private string SortUserFeaturedAlbumsUri { get; set; }
        private string UserTasksUri { get; set; }
        private string UserWatermarksUri { get; set; }
        private string UserPrintmarksUri { get; set; }
        private string UserUploadLimitsUri { get; set; }
        private string UserCouponsUri { get; set; }
        private string UserLatestQuickNewsUri { get; set; }
        private string UserGuideStatesUri { get; set; }
        private string UserHideGuidesUri { get; set; }
        private string FeaturesUri { get; set; }
        private string UserGrantsUri { get; set; }
        private string DuplicateImageSearchUri { get; set; }
        private string UserDeletedAlbumsUri { get; set; }
        private string UserDeletedFoldersUri { get; set; }
        private string UserDeletedPagesUri { get; set; }
        private string UserContactsUri { get; set; }
        #endregion

        public static User FromJson(string response)
        {
            var responseObj = JObject.Parse(response);
            var jUserObj = responseObj["Response"]["User"];
            var jUserUris = jUserObj["Uris"];

            var user = JsonConvert.DeserializeObject<User>(jUserObj.ToString());

            if (jUserUris != null)
            {
                user.BioImageUri = jUserUris["BioImage"]["Uri"].ToString();
                user.CoverImageUri = jUserUris["CoverImage"]["Uri"].ToString();
                user.UserProfileUri = jUserUris["UserProfile"]["Uri"].ToString();
                user.NodeUri = jUserUris["Node"]["Uri"].ToString();
                user.FolderUri = jUserUris["Folder"]["Uri"].ToString();
                user.UserAlbumsUri = jUserUris["UserAlbums"]["Uri"].ToString();
                user.UserGeoMediaUri = jUserUris["UserGeoMedia"]["Uri"].ToString();
                user.UserPopularMediaUri = jUserUris["UserPopularMedia"]["Uri"].ToString();
                user.UserFeaturedAlbumsUri = jUserUris["UserFeaturedAlbums"]["Uri"].ToString();
                user.UserRecentImagesUri = jUserUris["UserRecentImages"]["Uri"].ToString();
                user.UserImageSearchUri = jUserUris["UserImageSearch"]["Uri"].ToString();
                user.UserTopKeywordsUri = jUserUris["UserTopKeywords"]["Uri"].ToString();
                user.UrlPathLookupUri = jUserUris["UrlPathLookup"]["Uri"].ToString();
                user.UserAlbumTemplatesUri = jUserUris["UserAlbumTemplates"]["Uri"].ToString();
                user.SortUserFeaturedAlbumsUri = jUserUris["SortUserFeaturedAlbums"]["Uri"].ToString();
                user.UserTasksUri = jUserUris["UserTasks"]["Uri"].ToString();
                user.UserWatermarksUri = jUserUris["UserWatermarks"]["Uri"].ToString();
                user.UserPrintmarksUri = jUserUris["UserPrintmarks"]["Uri"].ToString();
                user.UserUploadLimitsUri = jUserUris["UserUploadLimits"]["Uri"].ToString();
                user.UserCouponsUri = jUserUris["UserCoupons"]["Uri"].ToString();
                user.UserLatestQuickNewsUri = jUserUris["UserLatestQuickNews"]["Uri"].ToString();
                user.UserGuideStatesUri = jUserUris["UserGuideStates"]["Uri"].ToString();
                user.UserHideGuidesUri = jUserUris["UserHideGuides"]["Uri"].ToString();
                user.FeaturesUri = jUserUris["Features"]["Uri"].ToString();
                user.UserGrantsUri = jUserUris["UserGrants"]["Uri"].ToString();
                user.DuplicateImageSearchUri = jUserUris["DuplicateImageSearch"]["Uri"].ToString();
                user.UserDeletedAlbumsUri = jUserUris["UserDeletedAlbums"]["Uri"].ToString();
                user.UserDeletedFoldersUri = jUserUris["UserDeletedFolders"]["Uri"].ToString();
                user.UserDeletedPagesUri = jUserUris["UserDeletedPages"]["Uri"].ToString();
                user.UserContactsUri = jUserUris["UserContacts"]["Uri"].ToString();
            }

            return user;
        }

        public async Task<Image> GetBioImage()
        {
            var url = $"{SmugMug.BaseUrl}{BioImageUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(url);

            return Image.FromJson(response, "BioImage");
        }

        public async Task<Image> GetCoverImage()
        {
            var url = $"{SmugMug.BaseUrl}{CoverImageUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(url);

            return Image.FromJson(response, "CoverImage");
        }

        public async Task<List<Image>> GetRecentImages()
        {                     
            var url = $"{SmugMug.BaseUrl}{CoverImageUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(url);

            return Image.ListFromJson(response);
        }

        public async Task<Node> GetRootNode()
        {
            var rootNodeUrl = $"{SmugMug.BaseUrl}{NodeUri}";
            var response = await SmugMug.GetResponseForProtectedRequest(rootNodeUrl);

            return Node.FromJson(response);
        }
    }
}
