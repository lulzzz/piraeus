﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Piraeus.Configuration;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()               
                .UseKestrel(options =>
                {
                    PiraeusConfig config = GetPiraeusConfig();
                    options.Limits.MaxConcurrentConnections = config.MaxConnections;
                    options.Limits.MaxConcurrentUpgradedConnections = config.MaxConnections;
                    options.Limits.MaxRequestBodySize = config.MaxBufferSize;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

                    X509Certificate2 cert = config.GetServerCerticate();
                    int[] ports = config.GetPorts();

                    foreach (int port in ports)
                    {
                        if (cert != null)
                        {
                            options.ListenAnyIP(port, (a) => a.UseHttps(cert));
                        }
                        else
                        {
                            IPAddress address = GetIPAddress(Dns.GetHostName());
                            options.Listen(address, port);
                            //options.Listen(IPAddress.Loopback, port);
                            //options.ListenAnyIP(port);
                        }
                    }

                    
                    if (!string.IsNullOrEmpty(config.ServerCertificateFilename))
                    {
                        string[] portStrings = config.Ports.Split(";", StringSplitOptions.RemoveEmptyEntries);

                        foreach (var portString in portStrings)
                        {
                            options.ListenAnyIP(Convert.ToInt32(portString), (a) => a.UseHttps(config.ServerCertificateFilename, config.ServerCertificatePassword));
                        }
                    }
                    //else
                    //{
                    //    options.ListenAnyIP(config.Channels.Http.ListenPort, (a) => a.UseHttps(".\\localhost.pfx", "pass@word1"));
                    //}                    
                });

        private static PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private static IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>();
    }
}
