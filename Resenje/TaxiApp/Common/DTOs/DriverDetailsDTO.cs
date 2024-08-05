using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class DriverDetailsDTO
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public bool IsBlocked { get; set; }


        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Common.Enums.UserVerificationStatus.Status Status { get; set; }

        public DriverDetailsDTO(string email, string name, string lastName, string username, bool isBlocked, double averageRating, Guid id, Common.Enums.UserVerificationStatus.Status status)
        {
            Email = email;
            Name = name;
            LastName = lastName;
            Username = username;
            IsBlocked = isBlocked;
            AverageRating = averageRating;
            Status = status;
            Id = id;
        }
    }
}
