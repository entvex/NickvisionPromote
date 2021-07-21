using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionPromote.Models
{
    public class PhoneNumberDatabase
    {
        private string _path;
        private string _failedNumbersPath;
        private string _optOutNumbersPath;
        private string _optInNumbersPath;

        public PhoneNumberDatabase()
        {
            _path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\phone.npromote";
            _failedNumbersPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\failed.txt";
            _optOutNumbersPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\optOut.txt";
            _optInNumbersPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote\\optIn.txt";
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote"))
            {
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionPromote");
            }
            if (!File.Exists(_path))
            {
                File.Create(_path).Dispose();
            }
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS phone (id INTEGER PRIMARY KEY, number TEXT)";
            command.ExecuteNonQuery();
        }

        public async Task<List<USPhoneNumber>> GetPhoneNumbersAsync()
        {
            var phoneNumbers = new List<USPhoneNumber>();
            using var connection = new SqliteConnection($"Data Source={_path}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM phone WHERE id > 0";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                phoneNumbers.Add(new USPhoneNumber(reader.GetString(1)));
            }
            return phoneNumbers;
        }

        public async Task<bool> AddPhoneNumberAsync(USPhoneNumber phoneNumber)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT OR IGNORE INTO phone VALUES($id,$num)";
            command.Parameters.AddWithValue("$id", phoneNumber.PhoneNumber);
            command.Parameters.AddWithValue("$num", phoneNumber.NumberString);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected == 1;
        }

        public async Task<int> ImportPhoneNumbersAsync(string pathToFile)
        {
            var successCount = 0;
            if (File.Exists(_failedNumbersPath))
            {
                File.Delete(_failedNumbersPath);
            }
            await Task.Run(async () =>
            {
                foreach (var line in await File.ReadAllLinesAsync(pathToFile))
                {
                    var hasNoLetters = true;
                    foreach (char ch in line)
                    {
                        if (ch >= 65 && ch <= 90)
                        {
                            hasNoLetters = false;
                        }
                        if (ch >= 97 && ch <= 122)
                        {
                            hasNoLetters = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(line) && hasNoLetters)
                    {
                        USPhoneNumber phoneNumber = null;
                        try
                        {
                            phoneNumber = new USPhoneNumber(line);
                        }
                        catch
                        {
                            await File.AppendAllTextAsync(_failedNumbersPath, line + "\n");
                            continue;
                        }
                        var addResult = await AddPhoneNumberAsync(phoneNumber);
                        if (addResult)
                        {
                            successCount++;
                        }
                    }
                }
            });
            return successCount;
        }

        public async Task<bool> DeletePhoneNumberAsync(USPhoneNumber phoneNumber)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM phone WHERE id = $id";
            command.Parameters.AddWithValue("$id", phoneNumber.PhoneNumber);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected == 1;
        }

        public async Task RemoveAllPhoneNumbersAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM phone WHERE id > 0";
            await command.ExecuteNonQueryAsync();
        }

        public async Task<(int OptIns, int OptOuts)> SyncOptInsAndOutsAsync()
        {
            var optIns = 0;
            var optOuts = 0;
            await Task.Run(async () =>
            {
                if(File.Exists(_optInNumbersPath))
                {
                    foreach (var line in await File.ReadAllLinesAsync(_optInNumbersPath))
                    {
                        var hasNoLetters = true;
                        foreach (char ch in line)
                        {
                            if (ch >= 65 && ch <= 90)
                            {
                                hasNoLetters = false;
                            }
                            if (ch >= 97 && ch <= 122)
                            {
                                hasNoLetters = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(line) && hasNoLetters)
                        {
                            USPhoneNumber phoneNumber = null;
                            try
                            {
                                phoneNumber = new USPhoneNumber(line);
                            }
                            catch { }
                            var addResult = await AddPhoneNumberAsync(phoneNumber);
                            if (addResult)
                            {
                                optIns++;
                            }
                        }
                    }
                }
            });
            await Task.Run(async () =>
            {
                if(File.Exists(_optOutNumbersPath))
                {
                    foreach (var line in await File.ReadAllLinesAsync(_optOutNumbersPath))
                    {
                        var hasNoLetters = true;
                        foreach (char ch in line)
                        {
                            if (ch >= 65 && ch <= 90)
                            {
                                hasNoLetters = false;
                            }
                            if (ch >= 97 && ch <= 122)
                            {
                                hasNoLetters = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(line) && hasNoLetters)
                        {
                            USPhoneNumber phoneNumber = null;
                            try
                            {
                                phoneNumber = new USPhoneNumber(line);
                            }
                            catch { }
                            var deleteResult = await DeletePhoneNumberAsync(phoneNumber);
                            if (deleteResult)
                            {
                                optOuts++;
                            }
                        }
                    }
                }
            });
            if (File.Exists(_optInNumbersPath))
            {
                File.Delete(_optInNumbersPath);
            }
            if (File.Exists(_optOutNumbersPath))
            {
                File.Delete(_optOutNumbersPath);
            }
            return (optIns, optOuts);
        }

        public async Task BackupAsync(string backupPath)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            await connection.OpenAsync();
            connection.BackupDatabase(new SqliteConnection($"Data Source={backupPath}"));
        }

        public async Task RestoreAsync(string backupPath)
        {
            using var backupConnection = new SqliteConnection($"Data Source={backupPath}");
            await backupConnection.OpenAsync();
            backupConnection.BackupDatabase(new SqliteConnection($"Data Source={_path}"));
        }

        public void OpenFailedImportsInNotepad() => Process.Start(new ProcessStartInfo() { FileName = "notepad.exe", Arguments = _failedNumbersPath, UseShellExecute = true });
    }
}
