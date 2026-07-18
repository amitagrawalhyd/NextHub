using NestHub.Mobile.ViewModels.Vendor;

namespace NestHub.Mobile.Views.Vendor;

public partial class SosLeadsPage : ContentPage
{
    private readonly VendorSosLeadsViewModel _viewModel;

    public SosLeadsPage(VendorSosLeadsViewModel viewModel)
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
