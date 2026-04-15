using StarPublications.Commands;
using System;
using System.Windows.Threading;
using System.Windows.Input;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages navigation between the four major sections:
    /// Dashboard, Sales Order Management, Book Search, and Publisher Search.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // ── Backing fields ──────────────────────────────────────────────────────
        private BaseViewModel _currentView;
        private string _statusMessage = "Ready";
        private string _currentTime = DateTime.Now.ToString("hh:mm:ss tt");
        private bool _isBusy;

        // ── Child ViewModels ─────────────────────────────────────────────────────
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly SalesOrderViewModel _salesOrderViewModel;
        private readonly BookSearchViewModel _bookSearchViewModel;
        private readonly PublisherSearchViewModel _publisherSearchViewModel;

        // ── Events / Delegates ───────────────────────────────────────────────────
        /// <summary>Delegate for navigation-change notifications.</summary>
        public delegate void NavigationChangedHandler(string viewName);

        /// <summary>Raised whenever the active view changes.</summary>
        public event NavigationChangedHandler? NavigationChanged;

        // ── Properties ───────────────────────────────────────────────────────────
        /// <summary>The currently displayed child view-model.</summary>
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    NavigationChanged?.Invoke(value?.GetType().Name ?? string.Empty);
                    // Notify active-nav indicator bindings
                    OnPropertyChanged(nameof(IsDashboardActive));
                    OnPropertyChanged(nameof(IsSalesOrdersActive));
                    OnPropertyChanged(nameof(IsBooksActive));
                    OnPropertyChanged(nameof(IsPublishersActive));
                }
            }
        }

        /// <summary>Status bar message shown at the bottom of the window.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>Live clock string updated every second.</summary>
        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        /// <summary>Indicates whether a long-running operation is in progress.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // ── Active-nav helpers ────────────────────────────────────────────────────
        public bool IsDashboardActive    => CurrentView is DashboardViewModel;
        public bool IsSalesOrdersActive  => CurrentView is SalesOrderViewModel;
        public bool IsBooksActive        => CurrentView is BookSearchViewModel;
        public bool IsPublishersActive   => CurrentView is PublisherSearchViewModel;

        // ── Commands ─────────────────────────────────────────────────────────────
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowSalesOrdersCommand { get; }
        public ICommand ShowBooksCommand { get; }
        public ICommand ShowPublishersCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public MainViewModel()
        {
            _dashboardViewModel = new DashboardViewModel();
            _salesOrderViewModel = new SalesOrderViewModel();
            _bookSearchViewModel = new BookSearchViewModel();
            _publisherSearchViewModel = new PublisherSearchViewModel();

            // Forward status updates from child VMs
            _dashboardViewModel.StatusMessageChanged       += msg => StatusMessage = msg;
            _salesOrderViewModel.StatusMessageChanged      += msg => StatusMessage = msg;
            _bookSearchViewModel.StatusMessageChanged      += msg => StatusMessage = msg;
            _publisherSearchViewModel.StatusMessageChanged += msg => StatusMessage = msg;

            ShowDashboardCommand    = new RelayCommand(_ => NavigateTo(_dashboardViewModel));
            ShowSalesOrdersCommand  = new RelayCommand(_ => NavigateTo(_salesOrderViewModel));
            ShowBooksCommand        = new RelayCommand(_ => NavigateTo(_bookSearchViewModel));
            ShowPublishersCommand   = new RelayCommand(_ => NavigateTo(_publisherSearchViewModel));

            // Default view is the dashboard
            _currentView = _dashboardViewModel;

            // Live clock
            var clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clock.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("hh:mm:ss tt");
            clock.Start();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void NavigateTo(BaseViewModel vm)
        {
            CurrentView = vm;
            StatusMessage = $"Navigated to {GetFriendlyName(vm)}";
        }

        private static string GetFriendlyName(BaseViewModel vm) => vm switch
        {
            DashboardViewModel       => "Dashboard",
            SalesOrderViewModel      => "Sales Order Management",
            BookSearchViewModel      => "Book Search",
            PublisherSearchViewModel => "Publisher Search",
            _ => "Unknown"
        };
    }
}
