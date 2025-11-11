using Moodyali.Core.Services;
using Moodyali.Shared.DTOs.Mood;
using Moodyali.API.Configuration;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Moodyali.API.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly OpenAIConfig _config;

    public RecommendationService(IHttpClientFactory httpClientFactory, Configuration.OpenAIConfig config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<string> GetRecommendationAsync(List<MoodResponse> weeklyMoods)
    {
        if (string.IsNullOrEmpty(_config.ApiKey) || _config.ApiKey == "set your api key here")
        {
            return "Veuillez configurer la clé API OpenAI dans appsettings.json pour obtenir des recommandations personnalisées.";
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.openai.com/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

        var prompt = BuildPrompt(weeklyMoods);

        var requestBody = new
        {
            model = "gpt-4.1-mini", // Using a fast and capable model
            messages = new[]
            {
                new { role = "system", content = "You are a helpful and empathetic mood analysis assistant. Your goal is to analyze the user's mood data and provide a single, concise, and actionable recommendation for the next week to improve their performance and mood. The response must be in French and should not exceed 100 words." },
                new { role = "user", content = prompt }
            },
            max_tokens = 150,
            temperature = 0.7
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseString);
            
            var recommendation = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return recommendation ?? "Impossible de générer une recommandation.";
        }
        catch (HttpRequestException ex)
        {
            // Log the exception details for debugging, but return a user-friendly message
            Console.WriteLine($"OpenAI API call failed: {ex.Message}");
            return "Erreur lors de la communication avec l'API OpenAI. Veuillez vérifier votre clé API et votre connexion.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return "Une erreur inattendue est survenue lors de la génération de la recommandation.";
        }
    }

    private string BuildPrompt(List<MoodResponse> weeklyMoods)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyse les données d'humeur de l'utilisateur pour la semaine passée et propose une recommandation pour la semaine prochaine.");
        sb.AppendLine("Données d'humeur (Date | Emoji | Score | Note) :");
        
        foreach (var mood in weeklyMoods.OrderBy(m => m.Date))
        {
            var noteText = string.IsNullOrEmpty(mood.Note) ? "Pas de note" : $"Note: \"{mood.Note}\"";
            sb.AppendLine($"{mood.Date.ToShortDateString()} | {mood.Emoji} | Score: {mood.Score} | {noteText}");
        }

        sb.AppendLine("\nBasé sur ces données, quelle est la meilleure recommandation pour améliorer l'humeur et la performance de l'utilisateur la semaine prochaine ? Réponds en français et sois concis (max 100 mots).");
        return sb.ToString();
    }
}
