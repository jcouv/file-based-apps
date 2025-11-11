#!/usr/bin/env dotnet

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

const string nugetOrgUrl = "https://azuresearch-usnc.nuget.org/query";

using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

try
{
    var dotnetTools = await GetMicrosoftDotNetTools(client);
    
    Console.WriteLine($"Found {dotnetTools.Count} Microsoft dotnet-tools:\n");
    
    foreach (var package in dotnetTools.OrderBy(p => p))
    {
        Console.WriteLine($"  - {package}");
    }
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Network error: {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

async Task<List<string>> GetMicrosoftDotNetTools(HttpClient httpClient)
{
    var allPackages = new List<string>();
    int skip = 0;
    const int take = 100;
    
    while (true)
    {
        var url = $"{nugetOrgUrl}?q=owner%3AMicrosoft&packageType=DotnetTool&skip={skip}&take={take}";
        
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        
        if (data.GetArrayLength() == 0)
        {
            break;
        }
        
        foreach (var item in data.EnumerateArray())
        {
            var id = item.GetProperty("id").GetString();
            if (!string.IsNullOrEmpty(id))
                allPackages.Add(id);
        }
        
        if (data.GetArrayLength() < take)
        {
            break;
        }
        
        skip += take;
    }
    
    return allPackages;
}
