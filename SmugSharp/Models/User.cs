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

        public static User FromResponse(string response)
        {
            var responseObj = JObject.Parse(response);
            var jUserObj = responseObj["Response"]["User"];
            var user = JsonConvert.DeserializeObject<User>(jUserObj.ToString());

            return user;
        }
    }
}
