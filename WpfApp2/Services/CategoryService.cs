using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    public class CategoryService:BaseService<Category>
    {
        private DatabaseService _db = new DatabaseService();

    }
}
