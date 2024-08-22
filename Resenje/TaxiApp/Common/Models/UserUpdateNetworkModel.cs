using Common.DTOs;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class UserUpdateNetworkModel //slicno UserModel kad sam pravio za registraciju, sluzi kao transportni model koji prenosi podatke preko mreze do servisa
    {

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public DateTime Birthday { get; set; }

        [DataMember]
        public string Email { get; set; }


        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public FileUploadRequestDTOs ImageFile { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string PreviousEmail { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        public UserUpdateNetworkModel(UserUpdateModel user) //konsturktor koji prima UserUpdateModel model koji prima podatke sa fronteda kad se menja profil
        {
            PreviousEmail = user.PreviousEmail;

            Id = user.Id;
            if (user.Address != null) Address = user.Address;

            //mm-dd-yyyy
            if (user.Birthday != null) Birthday = DateTime.ParseExact(user.Birthday, "MM-dd-yyyy", CultureInfo.InvariantCulture);
            else Birthday = DateTime.MinValue;

            if (user.Email != null) Email = user.Email;

            if (user.FirstName != null) FirstName = user.FirstName;
            if (user.FirstName != null) LastName = user.LastName;

            if (user.LastName != null) Username = user.Username;
            if (user.ImageUrl != null) ImageFile = makeFileOverNetwork(user.ImageUrl);

            if (user.Password != null) Password = user.Password;

        }
        //zbog slanja fajla kroz mrezu
        public FileUploadRequestDTOs makeFileOverNetwork(IFormFile file) //isto kao i kod UserModel funkcija za fajl-sliku, u njoj imam ime fajla, tip sadrzaja, i bajt sadrzaja fajla
        {
            FileUploadRequestDTOs fileOverNetwork;

            using (var stream = file.OpenReadStream())
            {
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                fileOverNetwork = new FileUploadRequestDTOs(file.FileName, file.ContentType, fileContent);
            }

            return fileOverNetwork;
        }
    }
}
