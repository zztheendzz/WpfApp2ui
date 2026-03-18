using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO
{
    public class SearchResultDto
    {
        public int Id { get; set; }
        public string Source { get; set; }   // Model / Brand / Vendor...
        public string Text { get; set; }     // Tên hiển thị
        public object Data { get; set; }     // Object thật (Model, Brand...)
    }
}
