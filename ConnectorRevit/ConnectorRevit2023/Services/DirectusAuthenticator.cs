﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calc.ConnectorRevit.Views;
using Calc.Core.DirectusAPI;

namespace Calc.ConnectorRevit.Services
{
    public class DirectusAuthenticator
    {
        public async Task<Directus> ShowLoginWindowAsync()
        {

            var url = Properties.Settings.Default.Config1;
            var email = Properties.Settings.Default.Config2;
            var password = Properties.Settings.Default.Config3;

            var directus = new Directus();

            while (!directus.Authenticated)
            {
                using (var inputDialog = new StringInputDialog(url, email, password))
                {
                    if (inputDialog.ShowDialog() == DialogResult.OK)
                    {
                        url = inputDialog.DirectusUrl;
                        email = inputDialog.Email;
                        password = inputDialog.Password;
                    }
                    if (inputDialog.DialogResult == DialogResult.Cancel)
                    {
                        Debug.WriteLine("Directus login cancelled");
                        return null;
                    }
                }
                await directus.Authenticate(url, email, password);
            }

            if (!directus.Authenticated)
            {
                return null;
            }

            Properties.Settings.Default.Config1 = url;
            Properties.Settings.Default.Config2 = email;
            Properties.Settings.Default.Config3 = password;

            Properties.Settings.Default.Save();

            return directus;
        }

    }

}