using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class DatabaseService
    {
        // поменять потом localhost на название своего ноута или пк смотря где запускаю бд
        private readonly string _connectionString = "Server=localhost;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;";

        // Получение задач с фильтрами и поиском
        public List<TaskItem> GetTasks(string filterType, string searchText, string sortOrder)
        {
            var tasks = new List<TaskItem>();

            // Базовый запрос
            string query = "SELECT Id, Title, IsCompleted, CreatedAt FROM Tasks WHERE 1=1";

            // фильтр по статусу
            if (filterType == "Active")
                query += " AND IsCompleted = 0";
            else if (filterType == "Completed")
                query += " AND IsCompleted = 1";

            // поиск по тексту
            if (!string.IsNullOrEmpty(searchText))
            {
                query += " AND Title LIKE @SearchText";
            }

            // сортировка
            if (sortOrder == "DateAsc")
                query += " ORDER BY CreatedAt ASC";
            else if (sortOrder == "NameAsc")
                query += " ORDER BY Title ASC";
            else if (sortOrder == "NameDesc")
                query += " ORDER BY Title DESC";
            else
                query += " ORDER BY CreatedAt DESC"; // По умолчанию: новые сверху

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    //  параметр поиска, 
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        command.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = (int)reader["Id"],
                                Title = reader["Title"].ToString(),
                                IsCompleted = (bool)reader["IsCompleted"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            });
                        }
                    }
                }
            }
            return tasks;
        }

        public bool TaskExists(string title)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Tasks WHERE Title = @Title";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public void AddTask(string title)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Tasks (Title) VALUES (@Title)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ToggleTaskCompletion(int id, bool isCompleted)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE Tasks SET IsCompleted = @IsCompleted WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsCompleted", isCompleted);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateTask(int id, string newTitle)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE Tasks SET Title = @Title WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", newTitle);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTask(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Tasks WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}