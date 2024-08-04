using Common.Enums;
using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class UserDetailsDTO
    {

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public int SumOfRatings { get; set; }

        [DataMember]
        public int NumOfRatings { get; set; }

        [DataMember]
        public DateTime Birthday { get; set; }

        [DataMember]
        public string Email { get; set; }


        [DataMember]
        public bool IsVerified { get; set; }

        [DataMember]
        public bool IsBlocked { get; set; }


        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public UserRoleType.Roles Roles { get; set; }

        [DataMember]
        public FileUploadRequestDTO ImageFile { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public UserVerificationStatus.Status Status { get; set; }

        [DataMember]
        public Guid Id { get; set; }


        public UserDetailsDTO(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string username, UserRoleType.Roles roles, FileUploadRequestDTO imageFile, string password, UserVerificationStatus.Status status, Guid id)
        {
            Address = address;
            AverageRating = averageRating;
            SumOfRatings = sumOfRatings;
            NumOfRatings = numOfRatings;
            Birthday = birthday;
            Email = email;
            IsVerified = isVerified;
            IsBlocked = isBlocked;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Roles = roles;
            ImageFile = imageFile;
            Password = password;
            Status = status;
            Id = id;
        }

        public UserDetailsDTO()
        {
        }
    }
}
