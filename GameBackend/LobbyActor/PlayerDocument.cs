namespace LobbyActor
{
    using Newtonsoft.Json;

    public class PlayerDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "login_count")]
        public int LoginCount { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
