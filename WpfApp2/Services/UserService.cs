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

            string sql = @"
        SELECT Id, UserName, Password, Role
        FROM [User]
        WHERE UserName = @UserName
          AND IsActive = 1
    ";

            var user = conn.QueryFirstOrDefault<User>(sql, new { UserName = username });

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;
            if (user.IsActive == 0)
            {
                return null;
            }

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

            string sql = @"
        UPDATE User
        SET 
            UserName = @UserName,
            Password = @Password,
            Role = @Role
        WHERE Id = @Id
        ";

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
