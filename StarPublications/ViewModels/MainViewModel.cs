using StarPublications.Commands;
using System.Windows.Input;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages navigation between the three major subsections:
    /// Sales Order Management, Book Search, and Publisher Search.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // ── Backing fields ──────────────────────────────────────────────────────
        private BaseViewModel _currentView;
        private string _statusMessage = "Ready";
        private bool _isBusy;

        // ── Child ViewModels ─────────────────────────────────────────────────────
        private readonly SalesOrderViewModel _salesOrderViewModel;
        private readonly BookSearchViewModel _bookSearchViewModel;
        private readonly PublisherSearchViewModel _publisherSearchViewModel;

        // ── Events / Delegates ───────────────────────────────────────────────────
        /// <summary>
        /// Delegate for navigation-change notifications.
        /// </summary>
        public delegate void NavigationChangedHandler(string viewName);

        /// <summary>
        /// Raised whenever the active view changes.
        /// </summary>
        public event NavigationChangedHandler? NavigationChanged;

        // ── Properties ───────────────────────────────────────────────────────────
        /// <summary>
        /// The currently displayed child view-model (bound to a ContentControl in the main window).
        /// </summary>
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                    NavigationChanged?.Invoke(value?.GetType().Name ?? string.Empty);
            }
        }

        /// <summary>Status bar message shown at the bottom of the window.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>Indicates whether a long-running operation is in progress.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // ── Commands ─────────────────────────────────────────────────────────────
        public ICommand ShowSalesOrdersCommand { get; }
        public ICommand ShowBooksCommand { get; }
        public ICommand ShowPublishersCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public MainViewModel()
        {
            _salesOrderViewModel = new SalesOrderViewModel();
            _bookSearchViewModel = new BookSearchViewModel();
            _publisherSearchViewModel = new PublisherSearchViewModel();

            // Forward busy/status updates from child VMs
            _salesOrderViewModel.StatusMessageChanged += msg => StatusMessage = msg;
            _bookSearchViewModel.StatusMessageChanged += msg => StatusMessage = msg;
            _publisherSearchViewModel.StatusMessageChanged += msg => StatusMessage = msg;

            ShowSalesOrdersCommand = new RelayCommand(_ => NavigateTo(_salesOrderViewModel));
            ShowBooksCommand = new RelayCommand(_ => NavigateTo(_bookSearchViewModel));
            ShowPublishersCommand = new RelayCommand(_ => NavigateTo(_publisherSearchViewModel));

            // Default view
            _currentView = _salesOrderViewModel;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void NavigateTo(BaseViewModel vm)
        {
            CurrentView = vm;
            StatusMessage = $"Navigated to {GetFriendlyName(vm)}";
        }

        private static string GetFriendlyName(BaseViewModel vm) => vm switch
        {
            SalesOrderViewModel => "Sales Order Management",
            BookSearchViewModel => "Book Search",
            PublisherSearchViewModel => "Publisher Search",
            _ => "Unknown"
        };
    }
}
