﻿using System.Linq;
using AstroPhotoGallery.Services;
using Autofac;

namespace AstroPhotoGallery.DependencyResolution.Modules
{
   public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            TestService obj = null;// requred => loading services assembly !!! 

            //Resolve Services
            var executingServicesAssembly = System.Reflection.Assembly.Load("AstroPhotoGallery.Services");
            var executingInterfaceAssembly = System.Reflection.Assembly.Load("AstroPhotoGallery.Services.Interfaces"); // TODO: make project 
            System.Reflection.Assembly[] arr = { executingServicesAssembly, executingInterfaceAssembly };
            builder.RegisterAssemblyTypes(arr)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();
        }
    }
}
