using Microsoft.Data.Sqlite;

var dbPath = "../../BackEnd/app.db";

var connectionString = $"Data Source={dbPath}";

using var connection = new SqliteConnection(connectionString);

await connection.OpenAsync();

var command = connection.CreateCommand();

command.CommandText = """
    UPDATE Students
    SET UserId = 5
    WHERE Id = 4;
    """;

var rows = await command.ExecuteNonQueryAsync();

Console.WriteLine($"تعداد Studentهای اصلاح‌شده: {rows}");

if (rows == 1)
{
    Console.WriteLine("Student با موفقیت به student_test متصل شد.");
}
else
{
    Console.WriteLine("Student مورد نظر پیدا نشد.");
}