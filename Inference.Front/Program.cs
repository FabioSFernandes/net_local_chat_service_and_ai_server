var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["InferenceApi"] ?? "http://localhost:5018");
    client.Timeout = Timeout.InfiniteTimeSpan;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/chat", async (HttpContext context, IHttpClientFactory http) =>
{
    using var request = new HttpRequestMessage(HttpMethod.Post, "/chat")
    {
        Content = new StreamContent(context.Request.Body)
    };
    request.Content.Headers.ContentType = new("application/json");

    var client = http.CreateClient("api");
    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "text/event-stream";
    await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
});

app.Run();
