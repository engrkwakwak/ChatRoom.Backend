﻿using Shared.DataTransferObjects.Users;

namespace Shared.DataTransferObjects.Email
{
    public class EmailDto
    {
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Body { get; set; }
        public string? Subject { get; set; }
        public UserDto? User { get; set; }
    }
}
