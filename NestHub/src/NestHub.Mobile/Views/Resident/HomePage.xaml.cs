namespace NestHub.Mobile.Views.Resident;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnSearchVendorsClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ResidentShell/Search");
}
