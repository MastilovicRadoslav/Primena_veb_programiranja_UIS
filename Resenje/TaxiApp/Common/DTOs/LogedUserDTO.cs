using Common.Enums;
using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class LogedUserDTO
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public UserRoleType.Roles Roles { get; set; }

        public LogedUserDTO(Guid id, UserRoleType.Roles roles)
        {
            Id = id;
            Roles = roles;
        }

        public LogedUserDTO()
        {
        }
    }
}
