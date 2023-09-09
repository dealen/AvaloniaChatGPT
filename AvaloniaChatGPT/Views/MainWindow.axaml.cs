using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaChatGPT.ViewModels;

namespace AvaloniaChatGPT.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is not null && DataContext is MainViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }

        base.OnLoaded(e);
    }
}
