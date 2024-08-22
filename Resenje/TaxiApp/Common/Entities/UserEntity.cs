using Common.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Entities
{
    public class UserEntity : TableEntity //za rad sa tabelama, to jest upisujem korisnika u bazu podataka sa ovim, u kolekciji sa UserModel
    {
        public string Address { get; set; }


        public double AverageRating { get; set; }


        public int SumOfRatings { get; set; }


        public int NumOfRatings { get; set; }

        public DateTime Birthday { get; set; }


        public string Email { get; set; }


        public bool IsVerified { get; set; }


        public bool IsBlocked { get; set; }


        public string FirstName { get; set; }


        public string LastName { get; set; }

        public string Password { get; set; }


        public string Username { get; set; }

        //Azure Table Storage ne podržava direktno čuvanje enum vrednosti, jedan od primjera zasto se pravi poseban model UserEntity za upis korisnika u bazu podataka
        public string TypeOfUser { get; set; }

        public string ImageUrl { get; set; }
        //Azure Table Storage ne podržava direktno čuvanje enum vrednosti, jedan od primjera zasto se pravi poseban model UserEntity za upis korisnika u bazu podataka
        public string Status { get; set; }

        public Guid Id { get; set; }


        public UserEntity(UserModel u, string imageUrl) //kada se korisnik kreira sa slikom koja je vec upload u BlobStorage pa se URL moze odmah postaviti
        {
            RowKey = u.Username; // key username user-a
            PartitionKey = u.TypeOfUser.ToString(); // partition key je tip user-a
            Address = u.Address;
            AverageRating = u.AverageRating;
            SumOfRatings = u.SumOfRatings;
            NumOfRatings = u.NumOfRatings;
            Birthday = u.Birthday;
            Email = u.Email;
            IsVerified = u.IsVerified;
            IsBlocked = u.IsBlocked;
            FirstName = u.FirstName;
            LastName = u.LastName;
            Password = u.Password;
            Username = u.Username;
            TypeOfUser = u.TypeOfUser.ToString(); //konnverzija 
            Status = u.Status.ToString(); //konverzija
            ImageUrl = imageUrl; // lokacija slike u blobu
            Id = u.Id;

        }



        public UserEntity(UserModel u)
        {
            RowKey = u.Username;  // key username user-a
            PartitionKey = u.TypeOfUser.ToString(); // partition key je tip user-a
            Address = u.Address;
            AverageRating = u.AverageRating;
            SumOfRatings = u.SumOfRatings;
            NumOfRatings = u.NumOfRatings;
            Birthday = u.Birthday;
            Email = u.Email;
            IsVerified = u.IsVerified;
            IsBlocked = u.IsBlocked;
            FirstName = u.FirstName;
            LastName = u.LastName;
            Password = u.Password;
            Username = u.Username;
            TypeOfUser = u.TypeOfUser.ToString();
            Status = u.Status.ToString();
            ImageUrl = u.ImageUrl; //iz UserModel-a

        }

        public UserEntity()
        {
        }
    }
}
