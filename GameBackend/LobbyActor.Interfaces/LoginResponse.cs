namespace LobbyActor.Interfaces
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int LoginCount { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, LoginCount: {1}", this.Id, this.LoginCount);
        }
    }
}
