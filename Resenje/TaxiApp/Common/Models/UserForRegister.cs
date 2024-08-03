﻿using Microsoft.AspNetCore.Http;

namespace Common.Models
{
    public class UserForRegister
    {
        public UserForRegister()
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string TypeOfUser { get; set; }

        public string Username { get; set; }

    }
}
