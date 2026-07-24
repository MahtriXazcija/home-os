namespace HomeOS.Application.Assistant;

public static class PageAssistantResponder
{
    public static string Respond(string pageId, string message, string? detailLevel)
    {
        var page = PageAssistantContext.Describe(pageId);

        if (detailLevel == "detailed")
        {
            return page.Detailed;
        }
        if (detailLevel == "brief")
        {
            return page.Brief;
        }

        var lowerMessage = message.ToLowerInvariant();

        PageFaqEntry? bestMatch = null;
        var bestScore = 0;
        foreach (var entry in page.Faq)
        {
            var score = entry.Keywords.Count(k => lowerMessage.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = entry;
            }
        }

        if (bestMatch is not null)
        {
            return bestMatch.Answer;
        }

        return $"""
            I don't have a specific answer for that, but here's what this page does: {page.Brief}
            Try asking about a specific feature on this page, or tap "Explain this page" above for a fuller walkthrough.
            """;
    }
}
