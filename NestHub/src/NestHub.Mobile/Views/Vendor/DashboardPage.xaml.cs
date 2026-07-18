using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.ViewModels.Vendor;

namespace NestHub.Mobile.Views.Vendor;

public partial class DashboardPage : ContentPage
{
    private readonly VendorDashboardViewModel _viewModel;

    public DashboardPage(VendorDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.AppearingCommand.Execute(null);
    }

    private async void OnLanguageClicked(object? sender, EventArgs e)
    {
        var strings = LocalizationResourceManager.Instance;
        var languages = _viewModel.SupportedLanguages;

        var choice = await DisplayActionSheet(strings["ChooseLanguage"], strings["Cancel"], null, languages.Select(l => l.DisplayName).ToArray());
        var selected = languages.FirstOrDefault(l => l.DisplayName == choice);
        if (selected is not null)
            _viewModel.SelectLanguageCommand.Execute(selected.CultureCode);
    }
}
