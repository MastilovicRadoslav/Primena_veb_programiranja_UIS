using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class LoginUserDTO
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        public LoginUserDTO(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
