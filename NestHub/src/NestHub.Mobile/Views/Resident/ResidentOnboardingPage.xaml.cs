using NestHub.Mobile.ViewModels.Resident;

namespace NestHub.Mobile.Views.Resident;

public partial class ResidentOnboardingPage : ContentPage
{
    private readonly ResidentOnboardingViewModel _viewModel;

    public ResidentOnboardingPage(ResidentOnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.AppearingCommand.Execute(null);
    }
}
