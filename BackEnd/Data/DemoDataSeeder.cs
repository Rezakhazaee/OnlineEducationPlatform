using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var admin = await db.Users.FirstAsync(u => u.Username == "admin");

        async Task<List<User>> EnsureUsersAsync(string role, string prefix, string fullName, int target)
        {
            var users = await db.Users.Where(u => u.Role == role).OrderBy(u => u.Id).ToListAsync();

            for (var i = 1; users.Count < target && i <= 500; i++)
            {
                var username = $"{prefix}{i:00}";
                if (await db.Users.AnyAsync(u => u.Username == username))
                    continue;

                var user = new User
                {
                    FullName = $"{fullName} {i:00}",
                    Mobile = $"099{i:00000000}",
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
                users.Add(user);
            }

            return users;
        }

        var instructors = await EnsureUsersAsync("Instructor", "demo.instructor", "استاد آزمایشی", 10);
        var supports = await EnsureUsersAsync("Support", "demo.support", "پشتیبان آزمایشی", 10);
        var marketers = await EnsureUsersAsync("Marketer", "demo.marketer", "بازاریاب آزمایشی", 5);

        var studentCount = await db.Students.CountAsync();
        for (var i = 1; studentCount < 40 && i <= 1000; i++)
        {
            var username = $"demo.student{i:000}";
            var nationalCode = (9900000000L + i).ToString();

            if (await db.Users.AnyAsync(u => u.Username == username) ||
                await db.Students.AnyAsync(s => s.NationalCode == nationalCode))
                continue;

            var user = new User
            {
                FullName = $"دانشجوی آزمایشی {i:000}",
                Mobile = $"0919{i:0000000}",
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.Students.Add(new Student
            {
                UserId = user.Id,
                FirstName = "دانشجوی",
                LastName = $"آزمایشی {i:000}",
                NationalCode = nationalCode,
                Mobile = user.Mobile,
                Address = "نشانی آزمایشی",
                MarketingUserId = marketers[(i - 1) % marketers.Count].Id,
                CreatedByUserId = admin.Id,
                SupportUserId = supports[(i - 1) % supports.Count].Id,
                CreatedDate = DateTime.Now
            });

            await db.SaveChangesAsync();
            studentCount++;
        }

        var courses = await db.Courses.OrderBy(c => c.Id).ToListAsync();

        for (var i = 1; courses.Count < 15 && i <= 100; i++)
        {
            var title = $"دوره تکمیلی آزمایشی {i:00}";
            if (await db.Courses.AnyAsync(c => c.Title == title))
                continue;

            var course = new Course
            {
                Title = title,
                Description = $"دورهٔ آزمایشی آموزش‌یار شمارهٔ {i:00} برای بررسی ثبت‌نام و محتوای آموزشی.",
                Price = 1500000m + (i * 250000m),
                Capacity = 50,
                DeliveryType = i % 3 == 0 ? "InPerson" : i % 3 == 1 ? "Online" : "Hybrid",
                InstructorId = instructors[(i - 1) % instructors.Count].Id,
                IsActive = true
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
            courses.Add(course);
        }

        foreach (var course in courses)
        {
            if (!instructors.Any(u => u.Id == course.InstructorId))
                course.InstructorId = instructors[(course.Id - 1) % instructors.Count].Id;
        }
        await db.SaveChangesAsync();

        var lessonCount = await db.CourseLessons.CountAsync();
        var lessonNumber = 1;

        while (lessonCount < 20)
        {
            var added = false;

            foreach (var course in courses)
            {
                if (lessonCount >= 20)
                    break;

                var module = await db.CourseModules
                    .FirstOrDefaultAsync(m => m.CourseId == course.Id && m.SortOrder == 99);

                if (module == null)
                {
                    module = new CourseModule
                    {
                        CourseId = course.Id,
                        Title = "ماژول تکمیلی آزمایشی",
                        SortOrder = 99,
                        IsActive = true
                    };

                    db.CourseModules.Add(module);
                    await db.SaveChangesAsync();
                }

                var sortOrder = (await db.CourseLessons
                    .Where(l => l.CourseModuleId == module.Id)
                    .Select(l => (int?)l.SortOrder)
                    .MaxAsync() ?? 0) + 1;

                var title = $"درس تکمیلی آزمایشی {lessonNumber:00}";
                if (await db.CourseLessons.AnyAsync(l => l.CourseModuleId == module.Id && l.Title == title))
                {
                    lessonNumber++;
                    continue;
                }

                db.CourseLessons.Add(new CourseLesson
                {
                    CourseModuleId = module.Id,
                    Title = title,
                    Description = "محتوای آزمایشی برای بررسی نمایش درس در پنل دانشجو.",
                    ContentType = "Text",
                    ContentUrl = "",
                    Content = $"این محتوای آزمایشی درس شمارهٔ {lessonNumber:00} در آموزش‌یار است. شامل معرفی موضوع، اهداف یادگیری و نکات تمرینی.",
                    DurationMinutes = 15,
                    SortOrder = sortOrder,
                    IsActive = true,
                    IsFreePreview = true
                });

                await db.SaveChangesAsync();
                lessonCount++;
                lessonNumber++;
                added = true;
            }

            if (!added && lessonCount < 20)
                throw new InvalidOperationException("تکمیل درس‌های آزمایشی پیش نرفت؛ عملیات برای بررسی متوقف شد.");
        }

        var students = await db.Students.OrderBy(s => s.Id).ToListAsync();
        foreach (var student in students)
        {
            if (!supports.Any(u => u.Id == student.SupportUserId))
                student.SupportUserId = supports[(student.Id - 1) % supports.Count].Id;
            if (!marketers.Any(u => u.Id == student.MarketingUserId))
                student.MarketingUserId = marketers[(student.Id - 1) % marketers.Count].Id;
            if (student.CreatedByUserId <= 0)
                student.CreatedByUserId = admin.Id;
        }
        await db.SaveChangesAsync();

        var enrollmentCount = await db.Enrollments.CountAsync();

        foreach (var student in students)
        {
            if (enrollmentCount >= 50)
                break;

            foreach (var course in courses)
            {
                if (enrollmentCount >= 50)
                    break;

                if (await db.Enrollments.AnyAsync(e =>
                    e.StudentId == student.Id && e.CourseId == course.Id))
                    continue;

                var instructorId = course.InstructorId ?? instructors[0].Id;

                db.Enrollments.Add(new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    CoursePartnerOrganizationId = null,
                    SupportUserId = supports[enrollmentCount % supports.Count].Id,
                    InstructorId = instructorId,
                    StartDate = new DateTime(2026, 1, 1).AddDays(enrollmentCount * 3),
                    Status = "Active",
                    Description = "ثبت‌نام آزمایشی پایدار برای تست آموزش‌یار"
                });

                await db.SaveChangesAsync();
                enrollmentCount++;
            }
        }

        var allEnrollments = await db.Enrollments.Include(e => e.Course).ToListAsync();
        foreach (var enrollment in allEnrollments)
        {
            if (!supports.Any(u => u.Id == enrollment.SupportUserId))
                enrollment.SupportUserId = supports[(enrollment.Id - 1) % supports.Count].Id;
            if (!instructors.Any(u => u.Id == enrollment.InstructorId))
                enrollment.InstructorId = enrollment.Course?.InstructorId ?? instructors[(enrollment.Id - 1) % instructors.Count].Id;
        }
        await db.SaveChangesAsync();

        // Add five persistent sample video lessons without duplicating them on restart.
        var videoSamples = new[]
        {
            ("ویدئوی آموزشی نمونه 01", "https://storage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"),
            ("ویدئوی آموزشی نمونه 02", "https://storage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4"),
            ("ویدئوی آموزشی نمونه 03", "https://storage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4"),
            ("ویدئوی آموزشی نمونه 04", "https://storage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4"),
            ("ویدئوی آموزشی نمونه 05", "https://storage.googleapis.com/gtv-videos-bucket/sample/ForBiggerJoyrides.mp4")
        };

        var videoCourses = courses.OrderBy(c => c.Id).Take(videoSamples.Length).ToList();
        for (var i = 0; i < videoCourses.Count; i++)
        {
            var course = videoCourses[i];
            var sample = videoSamples[i];

            var module = await db.CourseModules
                .Where(m => m.CourseId == course.Id)
                .OrderBy(m => m.SortOrder)
                .FirstOrDefaultAsync();

            if (module is null)
                continue;

            if (await db.CourseLessons.AnyAsync(l =>
                l.CourseModuleId == module.Id && l.Title == sample.Item1))
                continue;

            var sortOrder = (await db.CourseLessons
                .Where(l => l.CourseModuleId == module.Id)
                .Select(l => (int?)l.SortOrder)
                .MaxAsync() ?? 0) + 1;

            db.CourseLessons.Add(new CourseLesson
            {
                CourseModuleId = module.Id,
                Title = sample.Item1,
                Description = "ویدئوی نمونه برای آزمایش پخش محتوای آموزشی در آموزش‌یار.",
                ContentType = "Video",
                ContentUrl = sample.Item2,
                Content = string.Empty,
                DurationMinutes = 5,
                IsActive = true,
                IsFreePreview = true,
                SortOrder = sortOrder
            });
        }
        await db.SaveChangesAsync();

        Console.WriteLine("===== DEMO DATA COUNTS =====");
        Console.WriteLine($"Students missing support: {await db.Students.CountAsync(s => !supports.Select(u => u.Id).Contains(s.SupportUserId ?? 0))}");
        Console.WriteLine($"Students missing marketer: {await db.Students.CountAsync(s => s.MarketingUserId == null || !marketers.Select(u => u.Id).Contains(s.MarketingUserId.Value))}");
        Console.WriteLine($"Courses missing instructor: {await db.Courses.CountAsync(c => c.InstructorId == null || !instructors.Select(u => u.Id).Contains(c.InstructorId.Value))}");
        Console.WriteLine($"Enrollments missing support: {await db.Enrollments.CountAsync(e => !supports.Select(u => u.Id).Contains(e.SupportUserId ?? 0))}");
        Console.WriteLine($"Enrollments missing instructor: {await db.Enrollments.CountAsync(e => !instructors.Select(u => u.Id).Contains(e.InstructorId ?? 0))}");
        Console.WriteLine($"Students: {await db.Students.CountAsync()}");
        Console.WriteLine($"Enrollments: {await db.Enrollments.CountAsync()}");
        Console.WriteLine($"Lessons: {await db.CourseLessons.CountAsync()}");
        Console.WriteLine($"Instructors: {await db.Users.CountAsync(u => u.Role == "Instructor")}");
        Console.WriteLine($"Support: {await db.Users.CountAsync(u => u.Role == "Support")}");
        Console.WriteLine($"Marketers: {await db.Users.CountAsync(u => u.Role == "Marketer")}");
        Console.WriteLine($"Courses: {await db.Courses.CountAsync()}");
    }
}
