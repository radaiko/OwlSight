using FluentAssertions;
using OwlSight.Core.Configuration;

namespace OwlSight.Core.Tests.Configuration;

public class ConfigLoaderTests
{
    [Fact]
    public void Load_WithDefaults_ReturnsDefaultConfig()
    {
        var loader = new ConfigLoader();
        var config = loader.Load();

        config.Llm.BaseUrl.Should().Be("https://api.openai.com/v1");
        config.Llm.Model.Should().Be("gpt-4o");
        config.Llm.MaxTokens.Should().Be(4096);
        config.Llm.Temperature.Should().Be(0.2f);
        config.Review.MinSeverity.Should().Be("Info");
        config.Review.MaxFilesPerBatch.Should().Be(10);
    }

    [Fact]
    public void Load_WithCliArgs_OverridesDefaults()
    {
        var loader = new ConfigLoader();
        var config = loader.Load(
            baseBranch: "develop",
            baseUrl: "http://localhost:11434/v1",
            apiKey: "test-key",
            model: "llama3",
            maxFilesPerBatch: 5);

        config.BaseBranch.Should().Be("develop");
        config.Llm.BaseUrl.Should().Be("http://localhost:11434/v1");
        config.Llm.ApiKey.Should().Be("test-key");
        config.Llm.Model.Should().Be("llama3");
        config.Review.MaxFilesPerBatch.Should().Be(5);
    }

    [Fact]
    public void Load_WithConfigFile_ReadsFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var owlsightDir = Path.Combine(tempDir, ".owlsight");
        Directory.CreateDirectory(owlsightDir);
        try
        {
            File.WriteAllText(Path.Combine(owlsightDir, "config.json"), """
                {
                  "llm": { "model": "gpt-3.5-turbo", "maxTokens": 2048 },
                  "review": { "minSeverity": "Warning" }
                }
                """);

            var loader = new ConfigLoader();
            var config = loader.Load(workingDir: tempDir);

            config.Llm.Model.Should().Be("gpt-3.5-turbo");
            config.Llm.MaxTokens.Should().Be(2048);
            config.Review.MinSeverity.Should().Be("Warning");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_CliArgsOverrideConfigFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var owlsightDir = Path.Combine(tempDir, ".owlsight");
        Directory.CreateDirectory(owlsightDir);
        try
        {
            File.WriteAllText(Path.Combine(owlsightDir, "config.json"), """
                { "llm": { "model": "gpt-3.5-turbo" } }
                """);

            var loader = new ConfigLoader();
            var config = loader.Load(model: "gpt-4o", workingDir: tempDir);

            config.Llm.Model.Should().Be("gpt-4o");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
