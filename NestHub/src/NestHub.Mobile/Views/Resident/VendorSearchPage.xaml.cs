using NestHub.Mobile.ViewModels.Resident;

namespace NestHub.Mobile.Views.Resident;

public partial class VendorSearchPage : ContentPage
{
    private readonly VendorSearchViewModel _viewModel;

    public VendorSearchPage(VendorSearchViewModel viewModel)
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
