using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ProjectS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = ConfigureBuilder(args);

            try
            {
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(ex, "Critical error occurred, host finished");
                throw;
            }
        }

        private static WebApplication ConfigureBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Подключение к базе данных
            string connection = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connection))
            {
                throw new Exception("Строка подключения пустая");
            }

            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

            builder.Services.AddControllers();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            //Получение списка пользователей
            app.MapGet("/api/clients", async (ApplicationContext db) =>
            {
                var clients = await db.Clients
                    .Select(client => new { client.Id, client.Name, client.Surname })
                    .ToListAsync();

                return clients;
            });
            

            //Получаем одного клиента 
            app.MapGet("/api/clients/{id:Guid}", async (Guid id, ApplicationContext db) =>
            {
                Client? client = await db.Clients.FirstOrDefaultAsync(u => u.Id == id);
                if (client == null)
                {
                    return Results.NotFound(new { message = "Пользователь не найден" });
                }
                else
                {
                    return Results.Json(client);
                }
            });

            //Добавление клиента в db
            app.MapPost("/api/clients", async (Client client, ApplicationContext db) =>
            {
                await db.Clients.AddAsync(client);
                await db.SaveChangesAsync();
                return client.Id;
            });

            //Удаление клиента 
            app.MapDelete("/api/clients/{id:guid}", async (Guid id, ApplicationContext db) =>
            {
                // получаем пользователя по id
                Client? client = await db.Clients.FirstOrDefaultAsync(u => u.Id == id);
                if (client == null)
                {
                    return Results.NotFound(new { message = "Пользователь не найден" });
                }
                else
                {
                    db.Clients.Remove(client);
                    await db.SaveChangesAsync();
                    return Results.Json(client);
                }
            });

            //Изменение данных о клиенте 
            app.MapPut("/api/clients", async (Client client, ApplicationContext db) =>
            {
                var user = await db.Clients.FirstOrDefaultAsync(u => u.Id == client.Id);

                if (user == null)
                {
                    return Results.NotFound(new { message = "Пользователь не найден" });
                }
                else
                {
                    user.Email = client.Email;
                    user.Number = client.Number;
                    user.Adress = client.Adress;
                    await db.SaveChangesAsync();
                    return Results.Json(user);
                }
            });

            app.UseHttpsRedirection();

            return app;
        }
    }
}