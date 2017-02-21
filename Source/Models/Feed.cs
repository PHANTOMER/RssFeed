using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Xml.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Models
{
    [BsonIgnoreExtraElements]
    public class FeedItem
    {
        private string _url;
        private string _title;
        private string _description;

        [XmlIgnore]
        public string BaseUrl => "https://freelancer.com";

        [BsonId]
        [JsonProperty("id")]
        [XmlElement("guid")]
        public string Id { get; set; }

        [XmlIgnore]
        [JsonProperty("jobString")]
        public string JobString { get; set; }

        [XmlElement("title")]
        [JsonProperty("title")]
        public string Title
        {
            get { return $"{_title} - {string.Join(", ", Categories)}"; }
            set { _title = value; }
        }

        [XmlElement("description")]
        [JsonProperty("appended_descr")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [XmlElement("link")]
        [JsonProperty("linkUrl")]
        public string Url
        {
            get { return $"{_url}"; }
            set { _url = value; }
        }

        [XmlElement("category")]
        public string[] Categories { get; set; }

        [XmlIgnore]
        public DateTime CreatedDate { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Feed
    {
        private string _title;
        private string _link;
        private string _description;

        [XmlIgnore]
        [BsonId]
        public ObjectId Id { get; set; }

        [XmlElement("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [XmlElement("link")]
        public string Link
        {
            get { return _link; }
            set { _link = value; }
        }

        [XmlElement("description")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [JsonProperty("data")]
        [XmlElement("item")]
        public FeedItem[] Items { get; set; }
    }

    [XmlRoot("rss")]
    public class Rss
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("channel")]
        public Feed Feed { get; set; }
    }
}
