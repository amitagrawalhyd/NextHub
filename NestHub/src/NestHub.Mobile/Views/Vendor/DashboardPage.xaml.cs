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
}
