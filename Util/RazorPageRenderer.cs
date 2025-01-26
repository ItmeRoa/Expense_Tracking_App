using RazorLight;

namespace expense_tracker.Util;

public class RazorPageRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorPageRenderer()
    {
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        string template = $"{templateName}.cshtml";
        try
        {
            return await _engine.CompileRenderAsync(template, model);
        }
        catch (System.Exception ex)
        {
            throw new InvalidOperationException($"Error rendering template {template}", ex);
        }
    }
}