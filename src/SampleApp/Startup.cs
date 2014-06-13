﻿using Microsoft.AspNet.Builder;
using System;

namespace SampleApp
{
    public class Startup
    {
        public void Configure(IBuilder app)
        {
            app.Run(async context =>
            {
                Console.WriteLine("{0} {1}{2}{3}",
                    context.Request.Method,
                    context.Request.PathBase,
                    context.Request.Path,
                    context.Request.QueryString);

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Hello world");
            });
        }
    }
}