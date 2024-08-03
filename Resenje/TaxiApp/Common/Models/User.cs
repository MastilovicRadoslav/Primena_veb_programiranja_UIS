using Common.DTOs;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Runtime.Serialization;


namespace Common.Models
{
    [DataContract]
    public class User
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
        public string Password { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public UserRoles.Roles TypeOfUser { get; set; }


        [DataMember]
        public FileUploadDTO ImageFile { get; set; }

        [DataMember]
        public VerificationStatus.Status Status { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        public string ImageUrl { get; set; }


        public User(UserForRegister userRegister)
        {
            FirstName = userRegister.FirstName;
            LastName = userRegister.LastName;
            Birthday = DateTime.ParseExact(userRegister.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            Address = userRegister.Address;
            Email = userRegister.Email;
            Password = userRegister.Password;
            TypeOfUser = Enum.TryParse<UserRoles.Roles>(userRegister.TypeOfUser, true, out var role) ? role : UserRoles.Roles.Rider;
            Username = userRegister.Username;
            Id = Guid.NewGuid();
            switch (TypeOfUser)
            {
                case UserRoles.Roles.Admin:
                    IsVerified = true;
                    break;
                case UserRoles.Roles.Rider:
                    IsVerified = true;
                    break;
                case UserRoles.Roles.Driver:
                    AverageRating = 0.0;
                    IsVerified = false;
                    NumOfRatings = 0;
                    SumOfRatings = 0;
                    IsBlocked = false;
                    Status = VerificationStatus.Status.Procesira;
                    break;

            }
            ImageFile = makeFileOverNetwork(userRegister.ImageUrl);
        }



        public User()
        {
        }

        public User(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, UserRoles.Roles typeOfUser, FileUploadDTO imageFile, Guid id)
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
            Password = password;
            Username = username;
            TypeOfUser = typeOfUser;
            ImageFile = imageFile;
            Id = id;
        }

        public User(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, UserRoles.Roles typeOfUser, FileUploadDTO imageFile, string imageUrl, VerificationStatus.Status s, Guid id) : this(address, averageRating, sumOfRatings, numOfRatings, birthday, email, isVerified, isBlocked, firstName, lastName, password, username, typeOfUser, imageFile, id)
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
            Password = password;
            Username = username;
            TypeOfUser = typeOfUser;
            ImageFile = imageFile;
            ImageUrl = imageUrl;
            Status = s;
            Id = id;
        }

        public static FileUploadDTO makeFileOverNetwork(IFormFile file)
        {
            FileUploadDTO fileOverNetwork;

            using (var stream = file.OpenReadStream())
            {
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                fileOverNetwork = new FileUploadDTO(file.FileName, file.ContentType, fileContent);
            }

            return fileOverNetwork;
        }
    }
}
