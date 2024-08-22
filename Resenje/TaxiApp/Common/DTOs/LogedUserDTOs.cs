using Common.Enums;
using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class LogedUserDTOs //pronadjeni korisnik u kolekciji IReliableDictionary koji se onda zajedno sa tokenom porukom salje na fronted
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public UserRoleType.Roles Roles { get; set; } //za claims u generisanju JWT tokena

        public LogedUserDTOs(Guid id, UserRoleType.Roles roles)
        {
            Id = id;
            Roles = roles;
        }

        public LogedUserDTOs()
        {
        }
    }
}
