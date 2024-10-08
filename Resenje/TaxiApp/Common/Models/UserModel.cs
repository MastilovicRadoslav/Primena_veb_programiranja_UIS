﻿using Common.DTOs;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Runtime.Serialization;


namespace Common.Models
{
    [DataContract]
    public class UserModel
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
        public UserRoleType.Roles TypeOfUser { get; set; }


        [DataMember]
        public FileUploadRequestDTOs ImageFile { get; set; } //za prenos podataka o fajlu, kad slika stigne sa fronteda na bekend konvertujem je preko funkcije u ovo da imam sve sto treba

        [DataMember]
        public UserVerificationStatus.Status Status { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        public string ImageUrl { get; set; } // fajl-slika koja je upload sa fronteda forme


        public UserModel(UserRegistrationModel userRegister) // za UserRegistrationModel napralvjen konstruktor
        {
            FirstName = userRegister.FirstName;
            LastName = userRegister.LastName;
            Birthday = DateTime.ParseExact(userRegister.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture); //konverzija
            Address = userRegister.Address;
            Email = userRegister.Email;
            Password = userRegister.Password; //hesirao sam je na frontedu mada sam mogao i ovde
            TypeOfUser = Enum.TryParse<UserRoleType.Roles>(userRegister.TypeOfUser, true, out var role) ? role : UserRoleType.Roles.Rider; //konverzija
            Username = userRegister.Username;
            Id = Guid.NewGuid();
            switch (TypeOfUser)
            {
                case UserRoleType.Roles.Admin:
                    IsVerified = true;
                    break;
                case UserRoleType.Roles.Rider:
                    IsVerified = true;
                    break;
                case UserRoleType.Roles.Driver:
                    AverageRating = 0.0;
                    IsVerified = false;
                    NumOfRatings = 0;
                    SumOfRatings = 0;
                    IsBlocked = false;
                    Status = UserVerificationStatus.Status.Procesira;
                    break;

            }
            ImageFile = makeFileOverNetwork(userRegister.ImageUrl); //konvertovanje slike fajla koji je poslat sa fonteda u prenosivi DTOs objekat koji sam napravio da sadrzi sve potrebne inf o fajlu za cuvanje ili prenos preko mreze
        }



        public UserModel()
        {
        }

        public UserModel(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, UserRoleType.Roles typeOfUser, FileUploadRequestDTOs imageFile, Guid id)
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

        public UserModel(string address, double averageRating, int sumOfRatings, int numOfRatings, DateTime birthday, string email, bool isVerified, bool isBlocked, string firstName, string lastName, string password, string username, UserRoleType.Roles typeOfUser, FileUploadRequestDTOs imageFile, string imageUrl, UserVerificationStatus.Status s, Guid id) : this(address, averageRating, sumOfRatings, numOfRatings, birthday, email, isVerified, isBlocked, firstName, lastName, password, username, typeOfUser, imageFile, id)
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

        public static FileUploadRequestDTOs makeFileOverNetwork(IFormFile file) //slanje slike na bekend i cuvanje u skladistu
        {
            FileUploadRequestDTOs fileOverNetwork;

            using (var stream = file.OpenReadStream())
            {
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream); //kopiranje
                    fileContent = memoryStream.ToArray(); //niz bajtova
                }

                fileOverNetwork = new FileUploadRequestDTOs(file.FileName, file.ContentType, fileContent);
            }

            return fileOverNetwork;
        }
    }
}
