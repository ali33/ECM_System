using System;
using System.Windows.Navigation;

namespace Ecm.Mvvm
{
    /// <summary>
    ///   Provide the way to navigate to pages in the application. This class is used in ViewModel classes
    ///   which require the App class provide the navigation points. In case of unit test, 
    ///   the navigation points can be ignored
    /// </summary>
    public static class NavigationHelper
    {
        public static void Initialize(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.RemoveBackEntry();
        }

        public static void Navigate(Uri source)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigate(source);
            }
        }

        public static void Navigate(Uri source, object state)
        {
            if (_navigationService != null)
            {
                _navigationService.Navigate(source, state);
            }
        }

        private static NavigationService _navigationService;
    }
}