namespace NestHub.Mobile.Views.Compliance;

public partial class HelpPage : ContentPage
{
    public HelpPage() => InitializeComponent();

    private async void OnPrivacyPolicyClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("PrivacyPolicy");

    private async void OnTermsClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("TermsAndConditions");
}
