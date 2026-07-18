using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.ViewModels.Resident;

namespace NestHub.Mobile.Views.Resident;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.AppearingCommand.Execute(null);
    }

    private async void OnSearchVendorsClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ResidentShell/Search");

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
