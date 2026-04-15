using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.modelDTO;
using WpfApp2.Services.improtExcel;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    public partial class ModelAnalysis : Page
    {
        public ModelAnalysis()
        {
            InitializeComponent();
            DataContext = new ModelAnalysisVm();
        }

        // ================= SEARCH =================
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ModelAnalysisVm vm)
            {
                if (e.Key == Key.Enter)
                {
                    vm.ConfirmSelection();

                    // 👉 ADD vào matrix
                    if (vm.SelectedSearchResult != null)
                    {
                        vm.AddModelToMatrix(vm.SelectedSearchResult.Id);

                        RefreshMatrixColumns(); // reload column
                    }

                    txtSearchModel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Down)
                {
                    if (vm.IsSearchDropDownOpen && lstModel.SelectedIndex < lstModel.Items.Count - 1)
                    {
                        lstModel.SelectedIndex++;
                        lstModel.ScrollIntoView(lstModel.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (vm.IsSearchDropDownOpen && lstModel.SelectedIndex > 0)
                    {
                        lstModel.SelectedIndex--;
                        lstModel.ScrollIntoView(lstModel.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    vm.IsSearchDropDownOpen = false;
                    e.Handled = true;
                }
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && DataContext is ModelAnalysisVm vm)
            {
                vm.SelectedSearchResult = item.DataContext as SearchResultDto;
                vm.ConfirmSelection();

                // 👉 ADD vào matrix
                if (vm.SelectedSearchResult != null)
                {
                    vm.AddModelToMatrix(vm.SelectedSearchResult.Id);
                    RefreshMatrixColumns();
                }
            }
        }

        // ================= MATRIX =================


        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ ViewModel
            if (!(DataContext is ModelAnalysisVm vm) || vm.MatrixData == null || vm.MatrixData.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mở hộp thoại chọn nơi lưu file
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"Matrix_Price_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var exportService = new ExportSv();
                    exportService.ExportModelMatrix(vm.MatrixData, sfd.FileName);

                    MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Không thể ghi file. Có thể file đang được mở bởi một ứng dụng khác.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra trong quá trình xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void dgMatrix_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMatrixColumns();
        }

        private void RefreshMatrixColumns()
        {
            if (!(DataContext is ModelAnalysisVm vm)) return;

            dgMatrix.ItemsSource = null; // 🔥 reset trước

            dgMatrix.Columns.Clear();
            BuildMatrixColumns();

            dgMatrix.ItemsSource = vm.MatrixData.Rows; // 🔥 set sau cùng
        }

        private void BuildMatrixColumns()
        {
            if (!(DataContext is ModelAnalysisVm vm)) return;
            if (vm.MatrixData == null) return;

            // Cột Model
            dgMatrix.Columns.Add(new DataGridTextColumn
            {
                Header = "Model Name",
                Binding = new Binding("ModelName"),
                
                Width = DataGridLength.Auto
            });
            dgMatrix.Columns.Add(new DataGridTextColumn
            {
                Header = "Model Code",
                Binding = new Binding("ModelCode"),
                Width = DataGridLength.Auto
            });

            // Dynamic vendor columns
            foreach (var vendor in vm.MatrixData.Vendors)
            {
                dgMatrix.Columns.Add(new DataGridTextColumn
                {
                    Header = vendor,
                    Binding = new Binding($"VendorPrices[{vendor}]")
                    {
                        StringFormat = "N0"
                    },
                    MinWidth = 120
                });
            }
        }

        // ================= COMBOBOX UX =================

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                cb.IsDropDownOpen = true;
            }
        }

        private void ComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                if (!cb.IsKeyboardFocusWithin)
                {
                    cb.Focus();
                    e.Handled = true;
                }

                cb.IsDropDownOpen = true;
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    comboBox.ApplyTemplate();

                    var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                    if (textBox != null)
                    {
                        textBox.Focus();
                        textBox.SelectionLength = 0;
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }
    }
}