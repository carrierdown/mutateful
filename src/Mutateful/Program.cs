var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConnections();
builder.Services.AddSignalR().AddMessagePackProtocol();
builder.Services.AddSingleton<ClipSet>();
builder.Services.AddSingleton<CommandHandler>();
// builder.Services.AddHostedService<TestService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MutatefulHub>("/mutatefulHub");
});

app.Run();
