using TodoApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql("name=ToDoDB", new MySqlServerVersion(new Version(8, 0, 36)))
);

 
builder.Services.AddControllers(); // הוסף את שירותי MVC
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
builder.Services.AddCors();
 
 app.UseCors(builder => builder
 .AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader()
);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var dbContext = services.GetRequiredService<ToDoDbContext>();



app.MapGet("/items", async (ToDoDbContext dbContext) =>
{
    // שליפת כל המשימות
    var items = await dbContext.Items.ToListAsync();
    return Results.Ok(items);
});
app.MapPost("/items", async (ToDoDbContext dbContext, Item item) =>
{
    // הוספת משימה חדשה
    dbContext.Items.Add(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});
app.MapPut("/items/{id}", async (ToDoDbContext dbContext, int id, Item item) =>
{
    // עדכון משימה
    if (id != item.Id)
    {
        return Results.BadRequest("Id mismatch");
    }

    dbContext.Entry(item).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});
app.MapDelete("/items/{id}", async (ToDoDbContext dbContext, int id) =>
{
    // מחיקת משימה
    var item = await dbContext.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});
app.Run();
