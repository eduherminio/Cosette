using Cosette.Tuner.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseKestrel();
        webBuilder.UseStartup<Startup>();
        webBuilder.UseUrls("http://0.0.0.0:42000/");
    })
    .Build()
    .Run();
