using System.Text.RegularExpressions;

namespace Receply.Application.AiEngine;

/// <summary>Cleans up AI-generated text before it goes out over WhatsApp. The model is told to use
/// WhatsApp's *bold* convention (see the system prompts in GenerateAiReplyCommandHandler and
/// GenerateStaffAiReplyCommandHandler), but it still slips into standard Markdown's **bold** often
/// enough that this needs enforcing in code too, not just in the prompt.</summary>
public static class WhatsAppTextFormatter
{
    public static string Normalize(string text) => Regex.Replace(text, @"\*\*(.+?)\*\*", "*$1*");
}
