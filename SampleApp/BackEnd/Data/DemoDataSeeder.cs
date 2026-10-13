using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data;

public static class DemoDataSeeder
{
    private const string DemoPassword = "Test@12345";
    private const string Prefix = "آزمایشی | ";

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var admin = await db.Users.FirstAsync(u => u.Role == "Admin");

        var nonAdminUsers = await db.Users.Where(u => u.Role != "Admin").ToListAsync();
        foreach (var user in nonAdminUsers)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);
        await db.SaveChangesAsync();

        var instructorNames = new[]
        {
            ("test.instructor", "استاد تست"),
            ("demo.instructor.02", "مریم احمدی"),
            ("demo.instructor.03", "محمد کریمی"),
            ("demo.instructor.04", "سارا محمدی"),
            ("demo.instructor.05", "علی حسینی")
        };

        var supportNames = new[]
        {
            ("test.support", "پشتیبان تست"),
            ("demo.support.02", "زهرا رضایی"),
            ("demo.support.03", "امیر جعفری"),
            ("demo.support.04", "نگار موسوی"),
            ("demo.support.05", "حسین مرادی")
        };

        var marketerNames = new[]
        {
            ("test.marketer", "بازاریاب تست"),
            ("demo.marketer.02", "ندا کریمی"),
            ("demo.marketer.03", "مهدی صادقی"),
            ("demo.marketer.04", "الهام حیدری"),
            ("demo.marketer.05", "آرمان کاظمی")
        };

        var instructors = new List<User>();
        foreach (var item in instructorNames)
            instructors.Add(await GetOrCreateUser(db, item.Item1, item.Item2, "Instructor"));

        var supports = new List<User>();
        foreach (var item in supportNames)
            supports.Add(await GetOrCreateUser(db, item.Item1, item.Item2, "Support"));

        var marketers = new List<User>();
        foreach (var item in marketerNames)
            marketers.Add(await GetOrCreateUser(db, item.Item1, item.Item2, "Marketer"));

        var firstNames = new[]
        {
            "علی", "رضا", "محمد", "مهدی", "حسین", "امیر", "امیرعلی", "آرمان",
            "پارسا", "سینا", "نیما", "آرین", "کیان", "یاسین", "عرفان",
            "زهرا", "فاطمه", "مریم", "سارا", "نگار", "نرگس", "نازنین",
            "هانیه", "الهام", "مهسا", "ریحانه", "ستایش", "ترانه", "حدیث", "یلدا"
        };

        var lastNames = new[]
        {
            "رضایی", "امینی", "محمدی", "کریمی", "حسینی", "مرادی", "جعفری",
            "موسوی", "صادقی", "حیدری", "کاظمی", "قاسمی", "رحیمی", "اکبری",
            "رستمی", "نوری", "اسماعیلی", "عباسی", "یزدانی", "باقری"
        };

        var students = new List<Student>();

        for (var i = 0; i < 50; i++)
        {
            var username = $"demo.student.{i + 1:000}";
            var nationalCode = (9900000000L + i).ToString();

            var user = await GetOrCreateUser(
                db, username,
                $"{firstNames[i % firstNames.Length]} {lastNames[i % lastNames.Length]}",
                "Student",
                $"0913{i + 1:0000000}");

            var student = await db.Students
                .FirstOrDefaultAsync(s => s.NationalCode == nationalCode);

            if (student == null)
            {
                student = new Student
                {
                    UserId = user.Id,
                    FirstName = firstNames[i % firstNames.Length],
                    LastName = lastNames[(i * 3) % lastNames.Length],
                    NationalCode = nationalCode,
                    BirthDate = new DateTime(1995 + (i % 12), 1 + (i % 12), 1 + (i % 27)),
                    Mobile = user.Mobile,
                    Address = $"مشهد، منطقه {(i % 12) + 1}، خیابان نمونه",
                    GuardianName = $"ولی {firstNames[i % firstNames.Length]}",
                    GuardianMobile = $"0914{i + 1:0000000}",
                    CreatedByUserId = admin.Id,
                    CreatedDate = DateTime.Today.AddDays(-i * 3),
                    SupportUserId = supports[i % supports.Count].Id,
                    MarketingUserId = marketers[i % marketers.Count].Id
                };

                db.Students.Add(student);
                await db.SaveChangesAsync();
            }

            students.Add(student);
        }

        var courseTitles = new[]
        {
            "مبانی تحلیل داده", "اکسل کاربردی", "مقدمات هوش مصنوعی",
            "برنامه‌نویسی سی‌شارپ", "طراحی وب مقدماتی", "پایگاه داده و SQL",
            "مدیریت پروژه", "مهارت‌های ارتباطی", "بازاریابی دیجیتال",
            "تولید محتوای آموزشی", "تحلیل کسب‌وکار", "Power BI مقدماتی",
            "مبانی امنیت اطلاعات", "طراحی تجربه کاربری", "کاربرد ابزارهای هوش مصنوعی"
        };

        var courseDescriptions = new[]
        {
            "آشنایی عملی با مفاهیم پایه، تمرین‌های مرحله‌ای و یک پروژه کاربردی.",
            "آموزش همراه با مثال، تمرین عملی و ارزیابی پایان فصل."
        };

        var courses = new List<Course>();

        for (var i = 0; i < courseTitles.Length; i++)
        {
            var title = Prefix + courseTitles[i];
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Title == title);

            if (course == null)
            {
                course = new Course
                {
                    Title = title,
                    Description = courseDescriptions[i % courseDescriptions.Length],
                    Price = 1500000 + (i * 250000),
                    DeliveryType = i < 6 ? "InPerson" : "Online",
                    InstructorId = instructors[i / 3].Id,
                    IsActive = true
                };

                db.Courses.Add(course);
                await db.SaveChangesAsync();
            }

            courses.Add(course);

            var module = await db.CourseModules
                .FirstOrDefaultAsync(m => m.CourseId == course.Id && m.SortOrder == 1);

            if (module == null)
            {
                module = new CourseModule
                {
                    CourseId = course.Id,
                    Title = $"بخش اول: {courseTitles[i]}",
                    SortOrder = 1,
                    IsActive = true
                };
                db.CourseModules.Add(module);
                await db.SaveChangesAsync();
            }

            if (!await db.CourseLessons.AnyAsync(l =>
                    l.CourseModuleId == module.Id && l.SortOrder == 1))
            {
                db.CourseLessons.Add(new CourseLesson
                {
                    CourseModuleId = module.Id,
                    Title = $"مفاهیم پایه {courseTitles[i]}",
                    Description = "درس متنی همراه با توضیح و تمرین پیشنهادی.",
                    ContentType = "Text",
                    Content = $"در این درس با اصول {courseTitles[i]} آشنا می‌شوید. ابتدا مفاهیم اصلی را مطالعه کنید، سپس نکات مهم را یادداشت کرده و تمرین عملی خود را انجام دهید.",
                    DurationMinutes = 20 + (i % 4) * 5,
                    SortOrder = 1,
                    IsActive = true,
                    IsFreePreview = true
                });
            }

            if (!await db.CourseLessons.AnyAsync(l =>
                    l.CourseModuleId == module.Id && l.SortOrder == 2))
            {
                db.CourseLessons.Add(new CourseLesson
                {
                    CourseModuleId = module.Id,
                    Title = $"ویدئوی آموزشی {courseTitles[i]}",
                    Description = "ویدئوی نمونه برای آزمودن نمایش محتوای ویدئویی.",
                    ContentType = "Video",
                    ContentUrl = "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4",
                    DurationMinutes = 2,
                    SortOrder = 2,
                    IsActive = true,
                    IsFreePreview = true
                });
            }

            if (!await db.CourseLessons.AnyAsync(l =>
                    l.CourseModuleId == module.Id && l.SortOrder == 3))
            {
                db.CourseLessons.Add(new CourseLesson
                {
                    CourseModuleId = module.Id,
                    Title = $"تمرین عملی {courseTitles[i]}",
                    Description = "تمرین پیشنهادی برای تثبیت مطالب آموخته‌شده.",
                    ContentType = "Text",
                    Content = "تمرین: سه نکته اصلی درس را بنویسید، یک نمونه واقعی پیدا کنید و نتیجه کار خود را بررسی کنید.",
                    DurationMinutes = 15,
                    SortOrder = 3,
                    IsActive = true,
                    IsFreePreview = false
                });
            }

            await db.SaveChangesAsync();
        }

        var enrollmentsCreated = 0;

        // دقیقاً ۳۰ ثبت‌نام حضوری
        for (var i = 0; i < 30; i++)
        {
            var course = courses[i % 6];
            enrollmentsCreated += await EnsureEnrollment(
                db, students[i % 20], course, supports, course.InstructorId!.Value, i);
        }

        // دقیقاً ۴۰ ثبت‌نام آنلاین
        for (var i = 0; i < 40; i++)
        {
            var course = courses[6 + (i % 9)];
            var student = students[20 + (i % 30)];

            enrollmentsCreated += await EnsureEnrollment(
                db, student, course, supports, course.InstructorId!.Value, i + 30);
        }

        var demoCourseIds = courses.Select(c => c.Id).ToArray();

        Console.WriteLine("داده‌ساز آزمایشی اجرا شد.");
        Console.WriteLine($"دانشجوی آزمایشی: {students.Count}");
        Console.WriteLine($"استاد: {instructors.Count} | پشتیبان: {supports.Count} | بازاریاب: {marketers.Count}");
        Console.WriteLine($"دوره آزمایشی: {courses.Count}");
        Console.WriteLine($"ثبت‌نام جدید در این اجرا: {enrollmentsCreated}");
        Console.WriteLine($"ثبت‌نام آزمایشی موجود: {await db.Enrollments.CountAsync(e => demoCourseIds.Contains(e.CourseId))}");
        Console.WriteLine("رمز مشترک حساب‌های demo: Test@12345");
    }

    private static async Task<User> GetOrCreateUser(
        ApplicationDbContext db, string username, string fullName,
        string role, string? mobile = null)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user != null)
            return user;

        user = new User
        {
            Username = username,
            FullName = fullName,
            Mobile = mobile ?? "09120000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<int> EnsureEnrollment(
        ApplicationDbContext db, Student student, Course course,
        List<User> supports, int instructorId, int index)
    {
        var existing = await db.Enrollments.FirstOrDefaultAsync(e =>
            e.StudentId == student.Id && e.CourseId == course.Id);

        if (existing != null)
            return 0;

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            SupportUserId = supports[index % supports.Count].Id,
            InstructorId = instructorId,
            StartDate = DateTime.Today.AddDays(-(index % 120)),
            Status = "Active",
            Description = $"ثبت‌نام آزمایشی {index + 1:000}"
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        var paidAmount = index % 3 == 0
            ? course.Price
            : index % 3 == 1 ? course.Price / 2 : 0;

        if (paidAmount > 0)
        {
            db.Payments.Add(new Payment
            {
                EnrollmentId = enrollment.Id,
                Amount = paidAmount,
                PaymentDate = enrollment.StartDate.AddDays(1),
                PaymentType = "DEMO-Payment-1",
                Description = "پرداخت آزمایشی",
                Status = "Paid"
            });
        }

        if (paidAmount < course.Price)
        {
            db.Payments.Add(new Payment
            {
                EnrollmentId = enrollment.Id,
                Amount = course.Price - paidAmount,
                PaymentDate = enrollment.StartDate.AddDays(10),
                PaymentType = "DEMO-Payment-2",
                Description = "قسط آزمایشی؛ در انتظار پرداخت",
                Status = "Pending"
            });
        }

        await db.SaveChangesAsync();
        return 1;
    }
}
