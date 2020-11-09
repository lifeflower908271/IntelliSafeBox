using System;
using Stylet;
using StyletIoC;
using SafeBox.Pages;
using Utilities;

namespace SafeBox
{
    public class Bootstrapper : Bootstrapper<WelcomeViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
            UDG_.Initialize();
        }
    }
}
