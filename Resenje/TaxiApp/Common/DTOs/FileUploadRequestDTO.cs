using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class FileUploadRequestDTO
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] FileContent { get; set; }

        public FileUploadRequestDTO(byte[] fileContent)
        {
            FileContent = fileContent;
        }

        public FileUploadRequestDTO(string fileName, string contentType, byte[] fileContent)
        {
            FileName = fileName;
            ContentType = contentType;
            FileContent = fileContent;
        }

        public FileUploadRequestDTO()
        {
        }
    }
}
