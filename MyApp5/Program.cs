using Microsoft.AspNetCore.Mvc;
using MyApp5;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logger.txt"));
var app = builder.Build();


app.Use(async (HttpContext context, RequestDelegate next) =>
{
    try
    {
        await next.Invoke(context);
    }
    catch (Exception exception)
    {
        var logger = app.Logger;
        var time = DateTime.Now.ToString();

        var log = new StringBuilder();
        log.AppendLine($"Час: {time}");
        log.AppendLine($"Помилка: {exception}");
        log.AppendLine($"Request Path: {context.Request.Path}");
        log.AppendLine($"Request Method: {context.Request.Method}");

        if (context.Request.HasFormContentType && context.Request.Form.Any())
        {
            foreach (var form in context.Request.Form)
            {
                log.AppendLine($"Form Data: {form.Key} = {form.Value}");
            }
        }


        logger.LogError(log.ToString());
        throw;
    }
});
app.MapGet("/",async context =>
{
    StringBuilder sb = new StringBuilder();
   
    sb.Append("<html>" +
        "<body>" +
        "<form method=\"post\" action=\"/cookie\">" +
        "<div>" +
        "<label for=\"value\">Введіть значення:</label>" +
        "</div>" +
        "<div>" +
        "<input type=\"text\" name =\"value\" >" +
        "<label for=\"date\">Введіть дату</label>" +
        "</div>" +
        "<input type=\"date\" name=\"date\">" +
        "</div>" +
        "<div>" +
        "<input type=\"submit\">" +
        "</form>" +
        "</body>" +
        "</html>");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(sb.ToString());
}

);
app.MapPost("/cookie", async(context) => {

    StringBuilder sb = new StringBuilder();
    var value = context.Request.Form["value"];
    var dateStr = context.Request.Form["date"];
    sb.Append("<html>");
    if (DateTime.Parse(dateStr) < DateTime.Now)
    {
        sb.Append($"<h1>Значення  \"{value}\"не збережено</h1>");
        await context.Response.WriteAsync(sb.ToString());
    }
    if (DateTime.TryParse(dateStr, out DateTime date))
    {
        context.Response.Cookies.Append("Cookie", value, new CookieOptions
        {
            Expires = date,
        });
        sb.Append("Value." + "<a href='/checkcookie/'>Check</a>");
    }
  
    sb.Append("</html>");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(sb.ToString());
 

});

app.MapGet("/checkcookie", async (context) =>
{
    var cookie = context.Request.Cookies["Cookie"];
    
    StringBuilder sb = new StringBuilder();

    sb.Append("<html>" +
        "<body>" +
        "<h1>Значення з Cookies:"+cookie+"</h1>" +
        "</body>" +
        "</html>");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(sb.ToString());
});



app.Run();
