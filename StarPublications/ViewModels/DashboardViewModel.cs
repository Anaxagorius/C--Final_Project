using Microsoft.EntityFrameworkCore;
using StarPublications.Commands;
using StarPublications.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the Dashboard home screen.
    /// Displays at-a-glance statistics and recent activity.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        /// <summary>Raised when the status message should be updated.</summary>
        public event Action<string>? StatusMessageChanged;

        // ── Backing fields ────────────────────────────────────────────────────────
        private int _totalBooks;
        private int _totalAuthors;
        private int _totalPublishers;
        private int _totalStores;
        private int _totalOrders;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _recentOrders = string.Empty;
        private string _topBooks = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────────
        public int TotalBooks
        {
            get => _totalBooks;
            set => SetProperty(ref _totalBooks, value);
        }

        public int TotalAuthors
        {
            get => _totalAuthors;
            set => SetProperty(ref _totalAuthors, value);
        }

        public int TotalPublishers
        {
            get => _totalPublishers;
            set => SetProperty(ref _totalPublishers, value);
        }

        public int TotalStores
        {
            get => _totalStores;
            set => SetProperty(ref _totalStores, value);
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>Formatted list of the 5 most recent sales orders.</summary>
        public string RecentOrders
        {
            get => _recentOrders;
            set => SetProperty(ref _recentOrders, value);
        }

        /// <summary>Formatted list of top 5 best-selling titles.</summary>
        public string TopBooks
        {
            get => _topBooks;
            set => SetProperty(ref _topBooks, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand RefreshCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        // ── Data Operations ───────────────────────────────────────────────────────

        private void LoadData()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                using var ctx = DbContextFactory.Create();

                var totalBooks = ctx.Titles.Count();
                var totalAuthors = ctx.Authors.Count();
                var totalPublishers = ctx.Publishers.Count();
                var totalStores = ctx.Stores.Count();
                var totalOrders = ctx.Sales.Count();

                var recentOrders = ctx.Sales
                    .Include(s => s.Store)
                    .Include(s => s.Title)
                    .OrderByDescending(s => s.OrdDate)
                    .Take(5)
                    .ToList();

                var topBooks = ctx.Sales
                    .Include(s => s.Title)
                    .GroupBy(s => new { s.TitleId, s.Title!.TitleName })
                    .Select(g => new { g.Key.TitleName, TotalQty = g.Sum(s => (int)s.Qty) })
                    .OrderByDescending(x => x.TotalQty)
                    .Take(5)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalBooks = totalBooks;
                    TotalAuthors = totalAuthors;
                    TotalPublishers = totalPublishers;
                    TotalStores = totalStores;
                    TotalOrders = totalOrders;

                    RecentOrders = recentOrders.Count > 0
                        ? string.Join(Environment.NewLine, recentOrders.Select(s =>
                            $"• [{s.OrdDate:yyyy-MM-dd}]  {s.Store?.StorName ?? s.StorId}  —  {s.Title?.TitleName ?? s.TitleId}  (Qty: {s.Qty})"))
                        : "No recent orders found.";

                    TopBooks = topBooks.Count > 0
                        ? string.Join(Environment.NewLine, topBooks.Select((x, i) =>
                            $"{i + 1}. {x.TitleName}  ({x.TotalQty:N0} units sold)"))
                        : "No sales data available.";
                });

                StatusMessageChanged?.Invoke($"Dashboard refreshed — {totalOrders} total orders across {totalBooks} titles.");
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                Application.Current.Dispatcher.Invoke(() =>
                    ErrorMessage = $"Could not load dashboard data: {message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
