using RazorLight;

namespace Personal_finance_tracker.utils;

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
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error rendering template {template}", ex);
        }
    }
}