using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // ==========================================
        // 1. Main Admin
        // ==========================================

        var admin = await db.Users.FirstOrDefaultAsync(u => u.Username == "admin");

        if (admin == null)
        {
            admin = new User
            {
                FullName = "System Administrator",
                Mobile = "09000000000",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 2. Test Instructor
        // ==========================================

        var instructor = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "test.instructor");

        if (instructor == null)
        {
            instructor = new User
            {
                FullName = "استاد تست",
                Mobile = "09120000001",
                Username = "test.instructor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "Instructor",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(instructor);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 3. Test Support
        // ==========================================

        var support = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "test.support");

        if (support == null)
        {
            support = new User
            {
                FullName = "پشتیبان تست",
                Mobile = "09120000002",
                Username = "test.support",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "Support",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(support);
        }

        // ==========================================
        // ==========================================
        // ==========================================
        // 5. Test Marketer
        // ==========================================

        var marketer = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "test.marketer");

        if (marketer == null)
        {
            marketer = new User
            {
                FullName = "بازاریاب تست",
                Mobile = "09120000007",
                Username = "test.marketer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "Marketer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(marketer);
            await db.SaveChangesAsync();
        }

        // 4. Test Education Staff
        // ==========================================

        var educationStaff = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "test.education");

        if (educationStaff == null)
        {
            educationStaff = new User
            {
                FullName = "کارشناس آموزش تست",
                Mobile = "09120000006",
                Username = "test.education",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "EducationStaff",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(educationStaff);
            await db.SaveChangesAsync();
        }


        // 4. Organization Settings
        // ==========================================

        var settings = await db.OrganizationSettings
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new OrganizationSettings
            {
                Name = "سازمان آزمایشی",
                Description = "اطلاعات آزمایشی سامانه",
                IsActive = true,
                PrimaryColor = "#568fa8",
                SecondaryColor = "#263d4a",
                ThemeIsActive = true
            };

            db.OrganizationSettings.Add(settings);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 5. Partner Organization
        // ==========================================

        var partner = await db.PartnerOrganizations
            .FirstOrDefaultAsync(p => p.ContractNumber == "ORG-TEST-001");

        if (partner == null)
        {
            partner = new PartnerOrganization
            {
                Name = "سازمان همکار آزمایشی",
                ContactPerson = "مسئول تست",
                ContactMobile = "09120000003",
                ContractNumber = "ORG-TEST-001",
                ContractStartDate = new DateTime(2026, 1, 1),
                ContractEndDate = new DateTime(2026, 12, 29),
                IsActive = true,
                Description = "سازمان همکار برای تست سامانه",
                CreatedDate = DateTime.Now
            };

            db.PartnerOrganizations.Add(partner);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 6. Course
        // ==========================================

        var course = await db.Courses
            .FirstOrDefaultAsync(c => c.Title == "دوره آزمایشی تحلیل داده");

        if (course == null)
        {
            course = new Course
            {
                Title = "دوره آزمایشی تحلیل داده",
                Description = "دوره آزمایشی برای تست ثبت‌نام، قرارداد و پرداخت",
                Price = 4000000,
                InstructorId = instructor.Id,
                IsActive = true
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        // ==========================================
        if (course.Capacity != 20)
        {
            course.Capacity = 20;
            await db.SaveChangesAsync();
        }


        if (course.DeliveryType != "Online")
        {
            course.DeliveryType = "Online";
            await db.SaveChangesAsync();
        }


        // 7. Course - Partner Organization Agreement
        // ==========================================

        var agreement = await db.CoursePartnerOrganizations
            .FirstOrDefaultAsync(x =>
                x.CourseId == course.Id &&
                x.PartnerOrganizationId == partner.Id);

        if (agreement == null)
        {
            agreement = new CoursePartnerOrganization
            {
                CourseId = course.Id,
                PartnerOrganizationId = partner.Id,
                ContractNumber = "TEST-001",
                AgreedPrice = 4000000,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 29),
                IsActive = true,
                Description = "قرارداد آزمایشی دوره"
            };

            db.CoursePartnerOrganizations.Add(agreement);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 8. Test Student User
        // ==========================================

        var studentUser = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "test.student");

        if (studentUser == null)
        {
            studentUser = new User
            {
                FullName = "دانشجوی تست",
                Mobile = "09120000004",
                Username = "test.student",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"),
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(studentUser);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 9. Test Student
        // ==========================================

        var student = await db.Students
            .FirstOrDefaultAsync(s => s.NationalCode == "0012345678");

        if (student == null)
        {
            student = new Student
            {
                UserId = studentUser.Id,
                FirstName = "دانشجوی",
                LastName = "تست",
                NationalCode = "0012345678",
                BirthDate = new DateTime(2000, 1, 1),
                Mobile = "09120000004",
                Address = "آدرس آزمایشی",
                GuardianName = "ولی تست",
                GuardianMobile = "09120000005",
                CreatedByUserId = admin.Id,
                SupportUserId = support.Id,
                CreatedDate = DateTime.Now
            };

            db.Students.Add(student);
            await db.SaveChangesAsync();
        }
        if (student.MarketingUserId != marketer.Id)
        {
            student.MarketingUserId = marketer.Id;
            await db.SaveChangesAsync();
        }


        // ==========================================
        // 10. Enrollment
        // ==========================================

        var enrollment = await db.Enrollments
            .FirstOrDefaultAsync(e =>
                e.StudentId == student.Id &&
                e.CourseId == course.Id);

        if (enrollment == null)
        {
            enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                CoursePartnerOrganizationId = agreement.Id,
                SupportUserId = support.Id,
                InstructorId = instructor.Id,
                StartDate = new DateTime(2026, 9, 1),
                Status = "Active",
                Description = "ثبت نام آزمایشی برای تست پرداخت"
            };

            db.Enrollments.Add(enrollment);
            await db.SaveChangesAsync();
        }

        // ==========================================
        // ==========================================
        // 10.1. Installment Schedule
        // ==========================================

        var installmentExists = await db.Installments
            .AnyAsync(i => i.EnrollmentId == enrollment.Id);

        if (!installmentExists)
        {
            db.Installments.AddRange(
                new Installment
                {
                    EnrollmentId = enrollment.Id,
                    InstallmentNumber = 1,
                    Amount = 1000000,
                    DueDate = new DateTime(2026, 9, 1),
                    Status = "Paid",
                    Description = "قسط اول - داده تستی"
                },
                new Installment
                {
                    EnrollmentId = enrollment.Id,
                    InstallmentNumber = 2,
                    Amount = 1000000,
                    DueDate = new DateTime(2026, 10, 1),
                    Status = "Pending",
                    Description = "قسط دوم - داده تستی"
                },
                new Installment
                {
                    EnrollmentId = enrollment.Id,
                    InstallmentNumber = 3,
                    Amount = 2000000,
                    DueDate = new DateTime(2026, 11, 1),
                    Status = "Pending",
                    Description = "قسط سوم - داده تستی"
                });

            await db.SaveChangesAsync();
        }

        // 11. First Installment - Paid
        // ==========================================

        var firstPaymentExists = await db.Payments
            .AnyAsync(p =>
                p.EnrollmentId == enrollment.Id &&
                p.PaymentType == "FirstInstallment");

        if (!firstPaymentExists)
        {
            db.Payments.Add(new Payment
            {
                EnrollmentId = enrollment.Id,
                Amount = 1000000,
                PaymentDate = DateTime.Now,
                PaymentType = "FirstInstallment",
                Description = "قسط اول - داده تستی",
                Status = "Paid"
            });

            await db.SaveChangesAsync();
        }

        // ==========================================
        // 12. Second Installment - Pending
        // ==========================================

        var secondPaymentExists = await db.Payments
            .AnyAsync(p =>
                p.EnrollmentId == enrollment.Id &&
                p.PaymentType == "SecondInstallment");

        if (!secondPaymentExists)
        {
            db.Payments.Add(new Payment
            {
                EnrollmentId = enrollment.Id,
                Amount = 1000000,
                PaymentDate = DateTime.Now,
                PaymentType = "SecondInstallment",
                Description = "قسط دوم - برای تست Pending",
                Status = "Pending"
            });

            await db.SaveChangesAsync();
        }

        // ==========================================
        // ==========================================
        // 12.1. Link Payments to Installments
        // ==========================================

        var firstInstallment = await db.Installments
            .FirstOrDefaultAsync(i =>
                i.EnrollmentId == enrollment.Id &&
                i.InstallmentNumber == 1);

        var secondInstallment = await db.Installments
            .FirstOrDefaultAsync(i =>
                i.EnrollmentId == enrollment.Id &&
                i.InstallmentNumber == 2);

        var firstPayment = await db.Payments
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == enrollment.Id &&
                p.PaymentType == "FirstInstallment");

        var secondPayment = await db.Payments
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == enrollment.Id &&
                p.PaymentType == "SecondInstallment");

        if (firstPayment != null && firstInstallment != null &&
            firstPayment.InstallmentId != firstInstallment.Id)
        {
            firstPayment.InstallmentId = firstInstallment.Id;
        }

        if (secondPayment != null && secondInstallment != null &&
            secondPayment.InstallmentId != secondInstallment.Id)
        {
            secondPayment.InstallmentId = secondInstallment.Id;
        }

        await db.SaveChangesAsync();


        // 13. Test Course Module & Lessons
        // ==========================================

        var module = await db.CourseModules
            .FirstOrDefaultAsync(m => m.CourseId == course.Id && m.SortOrder == 1);

        if (module == null)
        {
            module = new CourseModule
            {
                CourseId = course.Id,
                Title = "ماژول تستی",
                SortOrder = 1,
                IsActive = true
            };

            db.CourseModules.Add(module);
            await db.SaveChangesAsync();
        }

        var lesson1 = await db.CourseLessons
            .FirstOrDefaultAsync(l => l.CourseModuleId == module.Id && l.SortOrder == 1);

        if (lesson1 == null)
        {
            db.CourseLessons.Add(new CourseLesson
            {
                CourseModuleId = module.Id,
                Title = "درس تستی اول",
                Description = "درس آزمایشی برای تست محتوای دوره",
                ContentType = "Text",
                ContentUrl = "",
                Content = "متن واقعی درس تستی آموزش‌یار برای بررسی نمایش محتوای متنی دوره.",
                DurationMinutes = 20,
                SortOrder = 1,
                IsActive = true,
                IsFreePreview = true
            });
        }


        if (lesson1 != null && string.IsNullOrWhiteSpace(lesson1.Content))
        {
            lesson1.Content = "متن واقعی درس تستی آموزش‌یار برای بررسی نمایش محتوای متنی دوره.";
            await db.SaveChangesAsync();
        }

        var lesson2 = await db.CourseLessons
            .FirstOrDefaultAsync(l => l.CourseModuleId == module.Id && l.SortOrder == 2);

        if (lesson2 == null)
        {
            db.CourseLessons.Add(new CourseLesson
            {
                CourseModuleId = module.Id,
                Title = "درس ویدئویی تست",
                Description = "درس آزمایشی برای تست پخش ویدئو",
                ContentType = "Video",
                ContentUrl = "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4",
                DurationMinutes = 1,
                SortOrder = 2,
                IsActive = true,
                IsFreePreview = true
            });
        }

        await db.SaveChangesAsync();
}

}
