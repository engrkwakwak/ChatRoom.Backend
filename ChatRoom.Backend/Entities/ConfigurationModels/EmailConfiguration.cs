﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ConfigurationModels
{
    public class EmailConfiguration
    {
        public string From { get; set; }  = String.Empty;
        public string Server { get; set; } = String.Empty;
        public int Port { get; set; } = 0;
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}