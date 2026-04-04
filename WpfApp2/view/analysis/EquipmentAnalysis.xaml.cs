using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2.modelDTO;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    public partial class EquipmentAnalysis : Page
    {
        private EquipmentAnalysisVm _viewModel;

        public EquipmentAnalysis()
        {
            InitializeComponent();
            _viewModel = new EquipmentAnalysisVm();
            this.DataContext = _viewModel;

            // Định dạng biểu đồ: Chia cho 1.000.000 và hiện hậu tố "Tr"
            // Đối với RowSeries (biểu đồ ngang), giá trị hiển thị phụ thuộc vào LabelFormatter của trục X
            if (ChartTopItems != null && ChartTopItems.AxisX.Count > 0)
            {
                ChartTopItems.AxisX[0].LabelFormatter = value => (value / 1000000).ToString("N1") + " Tr";
            }
        }

        /// <summary>
        /// Xử lý phím mũi tên lên/xuống, Enter và Escape trên ô tìm kiếm
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Nếu Popup không mở thì không xử lý các phím điều hướng danh sách
            if (!_viewModel.IsSearchDropDownOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    MoveSelection(1);
                    e.Handled = true;
                    break;

                case Key.Up:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    _viewModel.ConfirmSelection();
                    // Clear focus để đóng bàn phím ảo hoặc kết thúc trạng thái nhập liệu
                    Keyboard.ClearFocus();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    _viewModel.IsSearchDropDownOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Di chuyển mục đang chọn trong ListBox gợi ý
        /// </summary>
        private void MoveSelection(int direction)
        {
            if (lstSearchResults.Items.Count == 0) return;

            int nextIndex = lstSearchResults.SelectedIndex + direction;

            // Giới hạn index trong phạm vi danh sách
            if (nextIndex < 0) nextIndex = 0;
            if (nextIndex >= lstSearchResults.Items.Count) nextIndex = lstSearchResults.Items.Count - 1;

            lstSearchResults.SelectedIndex = nextIndex;
            // Tự động cuộn đến mục đang chọn nếu danh sách dài
            lstSearchResults.ScrollIntoView(lstSearchResults.SelectedItem);
        }

        /// <summary>
        /// Xử lý khi người dùng click chuột trực tiếp vào một mục trong danh sách gợi ý
        /// </summary>
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            // Tìm Item thực sự chứa dữ liệu (tránh click vào khoảng trắng của Border)
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is ListBoxItem item && item.DataContext is SearchResultDto selectedItem)
            {
                _viewModel.ConfirmSelection(selectedItem);
                // Sau khi chọn xong bằng chuột, trả lại focus cho TextBox hoặc Grid chính
                txtGlobalSearch.Focus();
            }
        }
    }
}