using System.Globalization;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NestHub.Mobile.Resources.Strings;

/// <summary>
/// Wraps the generated resx <see cref="ResourceManager"/> behind a live binding source. Views
/// bind via <c>{Binding [Key], Source={x:Static strings:LocalizationResourceManager.Instance}}</c>
/// — because it's a real property-changed-backed indexer rather than an <c>x:Static</c> lookup,
/// every bound string re-evaluates the instant <see cref="SetCulture"/> runs, with no page reload.
/// </summary>
public sealed record SupportedLanguage(string CultureCode, string DisplayName);

public sealed class LocalizationResourceManager : ObservableObject
{
    private const string CulturePreferenceKey = "AppCulture";

    public static LocalizationResourceManager Instance { get; } = new();

    public static IReadOnlyList<SupportedLanguage> SupportedLanguages { get; } = new[]
    {
        new SupportedLanguage("en", "English"),
        new SupportedLanguage("hi", "हिन्दी (Hindi)"),
        new SupportedLanguage("te", "తెలుగు (Telugu)"),
        new SupportedLanguage("or", "ଓଡ଼ିଆ (Odia)"),
        new SupportedLanguage("mr", "मराठी (Marathi)"),
    };

    private readonly ResourceManager _resourceManager =
        new("NestHub.Mobile.Resources.Strings.AppStrings", typeof(LocalizationResourceManager).Assembly);

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    private LocalizationResourceManager()
    {
    }

    public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? key;

    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>Restores the last-chosen language (or the device default) — call once at app startup.</summary>
    public void RestoreSavedCulture()
    {
        var savedCultureName = Preferences.Get(CulturePreferenceKey, string.Empty);
        SetCulture(string.IsNullOrEmpty(savedCultureName) ? CultureInfo.CurrentUICulture : new CultureInfo(savedCultureName));
    }

    public void SetCulture(CultureInfo culture)
    {
        _currentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;

        Preferences.Set(CulturePreferenceKey, culture.Name);

        // Signals every indexer binding in the app to re-pull its string for the new culture.
        OnPropertyChanged(string.Empty);
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(CurrentCulture));
    }
}
