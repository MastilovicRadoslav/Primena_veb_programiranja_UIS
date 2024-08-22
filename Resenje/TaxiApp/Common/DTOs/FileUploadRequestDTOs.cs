using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class FileUploadRequestDTOs //za formiranje fajla to jest slike sa imenom, tipom, i nizom bajtova
    {
        [DataMember]
        public string FileName { get; set; } //ime fajlsa

        [DataMember]
        public string ContentType { get; set; } //tip sadrzaja (image/jpeg)

        [DataMember]
        public byte[] FileContent { get; set; } //sadrzaj fajla kao niz bajtova, za cuvanje u bazi podataka ili slanje preko mreze

        public FileUploadRequestDTOs(byte[] fileContent) //MapUserEntityToUser
        {
            FileContent = fileContent;
        }

        public FileUploadRequestDTOs(string fileName, string contentType, byte[] fileContent) //UserModel, 
        {
            FileName = fileName;
            ContentType = contentType;
            FileContent = fileContent;
        }

        public FileUploadRequestDTOs()
        {
        }
    }
}
