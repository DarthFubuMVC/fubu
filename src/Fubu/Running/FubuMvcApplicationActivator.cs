﻿using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Services.Messaging;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Katana;
using FubuMVC.OwinHost;

namespace Fubu.Running
{
    public class FubuMvcApplicationActivator : IFubuMvcApplicationActivator
    {
        private IApplicationSource _applicationSource;
        private int _port;
        private string _physicalPath;
        private EmbeddedFubuMvcServer _server;

        public void Initialize(Type applicationType, int port, string physicalPath)
        {
            _applicationSource = Activator.CreateInstance(applicationType).As<IApplicationSource>();
            _port = PortFinder.FindPort(port);
            _physicalPath = physicalPath;

            StartUp();
        }

        public void StartUp()
        {
            try
            {
                var application = _applicationSource.BuildApplication();
                _server = new EmbeddedFubuMvcServer(application.Bootstrap(),
                                                    _physicalPath, _port);

                var list = new List<string>();
                PackageRegistry.Packages.Each(pak => pak.ForFolder(BottleFiles.WebContentFolder, list.Add));

                EventAggregator.SendMessage(new ApplicationStarted
                {
                    ApplicationName = _applicationSource.GetType().Name,
                    HomeAddress = _server.BaseAddress,
                    Timestamp = DateTime.Now,
                    BottleContentFolders = list.ToArray()
                });
            }
            catch (Exception e)
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    ExceptionText = e.ToString(),
                    Message = "Bootstrapping {0} Failed!".ToFormat(_applicationSource.GetType().Name)
                });
            }
        }

        public void ShutDown()
        {
            _server.SafeDispose();
        }

        public void Recycle()
        {
            ShutDown();
            StartUp();
        }
    }
}