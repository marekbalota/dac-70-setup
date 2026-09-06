using System.Globalization;

namespace DAC70Setup.BL.Features.Settings;

public record LanguageOption(string Name, CultureInfo Culture);
