using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmugSharp.Models
{
    public class Image
    {
        public string Uri { get; set; }
        public string WebUri { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public string Keywords { get; set; }
        public string[] KeywordArray { get; set; }
        public string Watermark { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Altitude { get; set; }
        public bool Hidden { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
        public bool Processing { get; set; }
        public string UploadKey { get; set; }
        public DateTime Date { get; set; }
        public string Format { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalSize { get; set; }
        public string LastUpdated { get; set; }
        public bool Collectable { get; set; }
        public bool IsArchive { get; set; }
        public bool IsVideo { get; set; }
        public bool CanEdit { get; set; }
        public bool Protected { get; set; }
        public string ImageKey { get; set; }
        public string ArchivedUri { get; set; }
        public long ArchivedSize { get; set; }
        public string ArchivedMD5 { get; set; }

        public string ImageUri { get; set; }
        public string ImageSizesUri { get; set; }

        public ImageSizes Sizes { get; set; }

        public async static Task<Image> FromJson(string response, string property = "Image")
        {
            var responseObj = JObject.Parse(response);
            var jObj = responseObj["Response"][property];

            Image image = null;
            if (jObj != null)
            {
                image = await FromJsonObject(jObj);
            }
            return image;
        }

        public async static Task<Image> FromJsonObject(JToken jObj)
        {
            Image image = null;
            var jUris = jObj["Uris"];
            image = JsonConvert.DeserializeObject<Image>(jObj.ToString());
            image.ImageSizesUri = jUris["ImageSizes"]["Uri"].ToString();
            if (!string.IsNullOrWhiteSpace(image.ImageSizesUri))
            {
                await image.GetSizes();
            }
            return image;
        }

        public async static Task<List<Image>> ListFromJson(string response)
        {
            var responseObj = JObject.Parse(response);
            var jObjs = responseObj["Response"]["Image"].Children().ToList();

            List<Image> images = new List<Image>();
            if (jObjs != null)
            {                                          
                Parallel.ForEach(jObjs, async j =>
                {
                    var image = await FromJsonObject(j);
                    images.Add(image);
                });
            }

            return images;
        }

        public async Task<ImageSizes> GetSizes()
        {
            if (!string.IsNullOrWhiteSpace(ImageSizesUri))
            {
                var response = await SmugMug.GetResponseForProtectedRequest($"{SmugMug.BaseUrl}{ImageSizesUri}");
                var responseObj = JObject.Parse(response);
                var jObj = responseObj["Response"]["ImageSizes"];

                if (jObj != null)
                {
                    Sizes = JsonConvert.DeserializeObject<ImageSizes>(jObj.ToString());
                }
            }

            return Sizes;
        }
    }

    public class ImageSizes
    {
        public string LargeImageUrl { get; set; }
        public string LargestImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string OriginalImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }
        public string TinyImageUrl { get; set; }
        public string X2LargeImageUrl { get; set; }
        public string X3LargeImageUrl { get; set; }
        public string XLargeImageUrl { get; set; }

    }
}
