﻿namespace RealtimeDatabase.Models.Commands
{
    public class LoginCommand : CommandBase
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
