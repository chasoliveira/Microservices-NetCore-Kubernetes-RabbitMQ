using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsProduction())
{
  Console.WriteLine("---> Using SqlServer Db...");
  builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
else
{
  Console.WriteLine("---> Using InMem Db...");
  builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

var commandServiceEndpoint = builder.Configuration["CommandService"];
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ICommandDataClient, CommandDataClient>(httpClient => httpClient.BaseAddress = new Uri(commandServiceEndpoint));

builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddGrpc();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints.MapControllers();
  endpoints.MapGrpcService<GrpcPlatformService>();
  endpoints.MapGet("/protos/platforms.proto", async context =>
  {
    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
  });
});

app.UseHealthChecks("/");

Console.WriteLine($"--> CommandService Endpoint: {commandServiceEndpoint}");
PrepDb.PrepPopulation(app, builder.Environment.IsProduction());

app.Run();
