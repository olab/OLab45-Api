using System;
using Newtonsoft.Json;

namespace OLabWebAPI.Dto
{
    public partial class AvatarsDto
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("map_id")]
        public uint MapId { get; set; }
        [JsonProperty("skin_1")]
        public string Skin1 { get; set; }
        [JsonProperty("skin_2")]
        public string Skin2 { get; set; }
        [JsonProperty("cloth")]
        public string Cloth { get; set; }
        [JsonProperty("nose")]
        public string Nose { get; set; }
        [JsonProperty("hair")]
        public string Hair { get; set; }
        [JsonProperty("environment")]
        public string Environment { get; set; }
        [JsonProperty("accessory_1")]
        public string Accessory1 { get; set; }
        [JsonProperty("bkd")]
        public string Bkd { get; set; }
        [JsonProperty("sex")]
        public string Sex { get; set; }
        [JsonProperty("mouth")]
        public string Mouth { get; set; }
        [JsonProperty("outfit")]
        public string Outfit { get; set; }
        [JsonProperty("bubble")]
        public string Bubble { get; set; }
        [JsonProperty("bubble_text")]
        public string BubbleText { get; set; }
        [JsonProperty("accessory_2")]
        public string Accessory2 { get; set; }
        [JsonProperty("accessory_3")]
        public string Accessory3 { get; set; }
        [JsonProperty("age")]
        public string Age { get; set; }
        [JsonProperty("eyes")]
        public string Eyes { get; set; }
        [JsonProperty("hair_color")]
        public string HairColor { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("is_private")]
        public int IsPrivate { get; set; }
    }
}
