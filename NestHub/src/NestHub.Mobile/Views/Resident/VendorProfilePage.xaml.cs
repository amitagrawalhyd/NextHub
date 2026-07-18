using NestHub.Mobile.ViewModels.Resident;

namespace NestHub.Mobile.Views.Resident;

public partial class VendorProfilePage : ContentPage
{
    private readonly VendorProfileViewModel _viewModel;

    public VendorProfilePage(VendorProfileViewModel viewModel)
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
