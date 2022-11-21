using System;
using System.Collections.Generic;

namespace CaptureMVC.Models
{
    public class LookupConnectionModel
    {
        public int DatabaseType { get; set; }

        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string DatabaseName { get; set; }

        public string Schema { get; set; }

        public int ProviderType { get; set; }

        public int Port { get; set; }

        public string ConnectionString { get; set; }

        public string SqlCommand { get; set; }

    }
}