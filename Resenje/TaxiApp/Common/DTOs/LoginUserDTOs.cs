using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class LoginUserDTOs //prima podatke sa fronteda kad se korisnik loguje
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        public LoginUserDTOs(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
