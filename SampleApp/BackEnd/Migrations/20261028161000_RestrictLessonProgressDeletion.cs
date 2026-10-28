using BackEnd.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BackEnd.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261028161000_RestrictLessonProgressDeletion")]
    public partial class RestrictLessonProgressDeletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "__new_LessonProgresses" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_LessonProgresses" PRIMARY KEY AUTOINCREMENT,
                    "EnrollmentId" INTEGER NOT NULL,
                    "CourseLessonId" INTEGER NOT NULL,
                    "IsCompleted" INTEGER NOT NULL,
                    "CompletedAt" TEXT NULL,
                    CONSTRAINT "FK_LessonProgresses_Enrollments_EnrollmentId"
                        FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_LessonProgresses_CourseLessons_CourseLessonId"
                        FOREIGN KEY ("CourseLessonId") REFERENCES "CourseLessons" ("Id") ON DELETE RESTRICT
                );
                INSERT INTO "__new_LessonProgresses"
                    ("Id", "EnrollmentId", "CourseLessonId", "IsCompleted", "CompletedAt")
                SELECT "Id", "EnrollmentId", "CourseLessonId", "IsCompleted", "CompletedAt"
                FROM "LessonProgresses";
                DROP TABLE "LessonProgresses";
                ALTER TABLE "__new_LessonProgresses" RENAME TO "LessonProgresses";
                CREATE INDEX "IX_LessonProgresses_CourseLessonId"
                    ON "LessonProgresses" ("CourseLessonId");
                CREATE UNIQUE INDEX "IX_LessonProgresses_EnrollmentId_CourseLessonId"
                    ON "LessonProgresses" ("EnrollmentId", "CourseLessonId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "__new_LessonProgresses" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_LessonProgresses" PRIMARY KEY AUTOINCREMENT,
                    "EnrollmentId" INTEGER NOT NULL,
                    "CourseLessonId" INTEGER NOT NULL,
                    "IsCompleted" INTEGER NOT NULL,
                    "CompletedAt" TEXT NULL,
                    CONSTRAINT "FK_LessonProgresses_Enrollments_EnrollmentId"
                        FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_LessonProgresses_CourseLessons_CourseLessonId"
                        FOREIGN KEY ("CourseLessonId") REFERENCES "CourseLessons" ("Id") ON DELETE CASCADE
                );
                INSERT INTO "__new_LessonProgresses"
                    ("Id", "EnrollmentId", "CourseLessonId", "IsCompleted", "CompletedAt")
                SELECT "Id", "EnrollmentId", "CourseLessonId", "IsCompleted", "CompletedAt"
                FROM "LessonProgresses";
                DROP TABLE "LessonProgresses";
                ALTER TABLE "__new_LessonProgresses" RENAME TO "LessonProgresses";
                CREATE INDEX "IX_LessonProgresses_CourseLessonId"
                    ON "LessonProgresses" ("CourseLessonId");
                CREATE UNIQUE INDEX "IX_LessonProgresses_EnrollmentId_CourseLessonId"
                    ON "LessonProgresses" ("EnrollmentId", "CourseLessonId");
                """);
        }
    }
}
