using Common.DTOs;
using Common.Entities;
using Common.Enums;
using Common.Models;
using static Common.Enums.UserVerificationStatus;

namespace Common.Mapper
{
    public class UserEntityMapper
    {
        //prima UserEtntiy model a vraca UserModel model, LoadUsers
        public static UserModel MapUserEntityToUser(UserEntity u, byte[] imageOfUser)//prima korisnika iz baze podataka i sliku za tog korisnika
        {
            var statusString = u.Status; // iz baze podataka je status u obliku stringa jer baza pdoataka podrzava samo stringove a ne enum
            Status myStatus; //ovo mi je enum

            if (Enum.TryParse(statusString, out myStatus)) //ako se uspjesno konvertuje string u enum
            {
                return new UserModel(
                    u.Address,
                    u.AverageRating,
                    u.SumOfRatings,
                    u.NumOfRatings,
                    u.Birthday,
                    u.Email,
                    u.IsVerified,
                    u.IsBlocked,
                    u.FirstName,
                    u.LastName,
                    u.Password,
                    u.Username,
                    (UserRoleType.Roles)Enum.Parse(typeof(UserRoleType.Roles), u.PartitionKey), //konvertujemo i ulogu jer kad smo upisivali to sam stavio da je PartitionKey
                    new FileUploadRequestDTOs(imageOfUser), //za sliku preko bajtova da je cuvam posebno smo izdvojili
                    u.ImageUrl,
                    myStatus,
                    u.Id
                );
            }
            return null;
        }
        //prima UserModel model a vraca FullUserDtos model
        public static FullUserDTOs MapUserToFullUserDto(UserModel u) //listUsers, changeUserFields, GetUserInfo
        {

            return new FullUserDTOs(
                u.Address,
                u.AverageRating,
                u.SumOfRatings,
                u.NumOfRatings,
                u.Birthday,
                u.Email,
                u.IsVerified,
                u.IsBlocked,
                u.FirstName,
                u.LastName,
                u.Username,
                u.TypeOfUser,
                u.ImageFile,
                u.Password,
                u.Status,
                u.Id);
        }
    }
}
