using NestHub.Mobile.ViewModels.Vendor;

namespace NestHub.Mobile.Views.Vendor;

public partial class VendorOnboardingPage : ContentPage
{
    private readonly VendorOnboardingViewModel _viewModel;

    public VendorOnboardingPage(VendorOnboardingViewModel viewModel)
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
