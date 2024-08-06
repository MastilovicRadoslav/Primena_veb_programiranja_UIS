using Common.DTOs;
using Common.Entities;
using Common.Enums;
using Common.Models;
using static Common.Enums.UserVerificationStatus;

namespace Common.Mappers
{
    public class UserEntityMapper
    {

        public static UserModel MapUserEntityToUser(UserEntity u, byte[] imageOfUser)
        {
            var statusString = u.Status; // Assuming 'u.Status' contains the string representation of the enum
            Status myStatus;

            if (Enum.TryParse(statusString, out myStatus))
            {
                // Successfully parsed the string to an enum
                // 'myStatus' now contains the corresponding enum value
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
                    (UserRoleType.Roles)Enum.Parse(typeof(UserRoleType.Roles), u.PartitionKey),
                    new FileUploadRequestDTO(imageOfUser),
                    u.ImageUrl,
                    myStatus,
                    u.Id
                );
            }
            return null;
        }

        public static UserDetailsDTO MapUserToFullUserDto(UserModel u)
        {

            return new UserDetailsDTO(u.Address, u.AverageRating, u.SumOfRatings, u.NumOfRatings, u.Birthday, u.Email, u.IsVerified, u.IsBlocked, u.FirstName, u.LastName, u.Username, u.TypeOfUser, u.ImageFile, u.Password, u.Status, u.Id);
        }
    }
}
