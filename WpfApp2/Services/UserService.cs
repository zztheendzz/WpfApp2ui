using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    public class UserService
    {
        public UserService() { }
        public DatabaseService _db = new DatabaseService();
        public User Login(string username, string password)
        {
            using var conn = _db.GetConnection();

            // 1. Lấy user theo UserName và phải còn Active
            // Thêm cột IsActive vào SELECT để kiểm tra phía dưới
            string sql = @"
        SELECT Id, UserName, Password, Role, IsActive
        FROM [User]
        WHERE UserName = @UserName AND IsActive = 1";

            var user = conn.QueryFirstOrDefault<User>(sql, new { UserName = username });

            // 2. Nếu không tìm thấy UserName hoặc User bị khóa (IsActive = 0)
            if (user == null)
            {
                // Bạn không nên nói rõ là "Tài khoản không tồn tại" để tránh bị dò quét user
                return null;
            }

            // 3. Kiểm tra mật khẩu bằng BCrypt (Chỉ gọi 1 lần duy nhất)
            bool isPasswordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (!isPasswordMatch)
            {
                // MessageBox này chỉ nên dùng để debug, khi chạy thật nên bỏ đi
                // MessageBox.Show("Mật khẩu không chính xác!"); 
                return null;
            }

            // 4. Đăng nhập thành công
            return user;
        }
        public IEnumerable<UserDto> GetUserDTO()
        {
            using var conn = _db.GetConnection();
            string sql = @"
                SELECT 
                    m.Id,
                    m.UserName,
                    m.Password,       
                    m.Role,
                    m.IsActive       
                FROM User m
                ";
            return conn.Query<UserDto>(sql);
        }

        public IEnumerable<UserDto> add()
        {
            using var conn = _db.GetConnection();
            string sql = @"
                SELECT 
                    m.Id,
                    m.UserName,
                    m.Password,      
                    m.Role,
                    b.IsActive    
                FROM User m
                ";
            return conn.Query<UserDto>(sql);
        }
        public void Delete(int id)
        {
            using var conn = _db.GetConnection();
            string sql = "DELETE FROM User WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(UserDto user)
        {
            using var conn = _db.GetConnection();

            // 1. Kiểm tra xem ViewModel có gửi mật khẩu mới (đã hash) xuống không
            bool hasNewPassword = !string.IsNullOrWhiteSpace(user.Password);

            string sql;
            if (hasNewPassword)
            {
                // Trường hợp 1: Có mật khẩu mới -> Cập nhật tất cả các cột
                sql = @"
            UPDATE [User]
            SET UserName = @UserName,
                Password = @Password,
                Role = @Role,
                IsActive = @IsActive
            WHERE Id = @Id";
            }
            else
            {
                // Trường hợp 2: Password rỗng -> Chỉ cập nhật các thông tin khác, GIỮ NGUYÊN Password cũ trong DB
                sql = @"
            UPDATE [User]
            SET UserName = @UserName,
                Role = @Role,
                IsActive = @IsActive
            WHERE Id = @Id";
            }

            // 2. Thực thi với Dapper
            conn.Execute(sql, user);
        }

        public int Add(UserDto user)
        {
            MessageBox.Show("user" + user.Role + " " +user.UserName);
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO User (UserName, IsActive,Role,Password)
    VALUES (@UserName, @IsActive, @Role,@Password);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, user);
        }
        

    }
}
