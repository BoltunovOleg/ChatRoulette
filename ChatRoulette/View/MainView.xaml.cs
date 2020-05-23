using System;
using System.Windows;
using System.Windows.Input;
using ChatRoulette.Ioc;
using ChatRoulette.ViewModel;
using MaterialDesignThemes.Wpf;

namespace ChatRoulette.View
{
    public partial class MainView
    {
        public MainView()
        {
            this.InitializeComponent();
            IocKernel.Bind<ISnackbarMessageQueue>().ToConstant(this.MainSnackbar.MessageQueue);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void DarkModeToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            ModifyTheme(theme => theme.SetBaseTheme(this.DarkModeToggleButton.IsChecked == true ? Theme.Dark : Theme.Light));
        }

        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void MainView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (this.ContentControl.Content is BaseViewModel bvm)
            {
                if (bvm.RedirectInput)
                {
                    e.Handled = await bvm.KeyDown(e.Key);
                }
            }
        }

        private async void MainView_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (this.ContentControl.Content is BaseViewModel bvm)
            {
                if (bvm.RedirectInput)
                {
                    e.Handled = await bvm.KeyUp(e.Key);
                }
            }
        }
    }
}