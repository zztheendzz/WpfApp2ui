using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    internal class CurrencyDto
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }
    }
}
