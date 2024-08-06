using Common.DTO;
using Common.DTOs;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class UserUpdateNetworkModel
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
        public FileUploadRequestDTO ImageFile { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string PreviousEmail { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        public UserUpdateNetworkModel(UserUpdateModel user)
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

        public FileUploadRequestDTO makeFileOverNetwork(IFormFile file)
        {
            FileUploadRequestDTO fileOverNetwork;

            using (var stream = file.OpenReadStream())
            {
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                fileOverNetwork = new FileUploadRequestDTO(file.FileName, file.ContentType, fileContent);
            }

            return fileOverNetwork;
        }
    }
}
