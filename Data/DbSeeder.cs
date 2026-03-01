using HizliOgren.Models;
using HizliOgren.Services;
using Microsoft.EntityFrameworkCore;

namespace HizliOgren.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, AuthService auth)
    {
        if (!await db.Users.AnyAsync())
        {
            var admin = new User
            {
                Email = "admin@hizliogren.net.tr",
                PasswordHash = auth.HashPassword("Admin123!"),
                PlainPassword = "Admin123!",
                FullName = "Site Yöneticisi",
                Role = UserRole.Admin
            };
            db.Users.Add(admin);

            var categories = new[]
            {
                new Category { Name = "İletişim", Slug = "iletisim", SortOrder = 1 },
                new Category { Name = "Yönetim ve Liderlik", Slug = "liderlik", SortOrder = 2 },
                new Category { Name = "Kişisel Gelişim", Slug = "kisisel", SortOrder = 3 },
                new Category { Name = "Satış Okulu", Slug = "satis", SortOrder = 4 },
                new Category { Name = "Sunum Becerileri", Slug = "sunum", SortOrder = 5 },
                new Category { Name = "Zaman Yönetimi", Slug = "zaman", SortOrder = 6 }
            };
            db.Categories.AddRange(categories);

            await db.SaveChangesAsync();
        }

        if (!await db.Courses.AnyAsync())
        {
            var admin = await db.Users.FirstAsync(u => u.Role == UserRole.Admin);
            var catIletisim = await db.Categories.FirstAsync(c => c.Slug == "iletisim");

            var instructor = new User
            {
                Email = "egitmen@hizliogren.net.tr",
                PasswordHash = auth.HashPassword("Egitmen123!"),
                PlainPassword = "Egitmen123!",
                FullName = "Dr. Ayşe Yılmaz",
                Role = UserRole.Egitmen
            };
            db.Users.Add(instructor);
            await db.SaveChangesAsync();

            var course = new Course
            {
                Title = "Çatışma Yönetimi",
                Description = "İş ve özel hayatınızda çatışmaları yapıcı şekilde yönetmeyi öğrenin. Bu eğitimde çatışma türleri, iletişim teknikleri ve pratik çözüm stratejileri anlatılmaktadır. Video dersler, indirilebilir PDF'ler ve makalelerle desteklenir.",
                CategoryId = catIletisim.Id,
                InstructorId = instructor.Id,
                Price = 199,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var items = new[]
            {
                new CurriculumItem { CourseId = course.Id, Type = "video", Title = "Giriş – Çatışma Nedir?", DurationMinutes = 2, SortOrder = 1 },
                new CurriculumItem { CourseId = course.Id, Type = "video", Title = "Çatışma Türleri", DurationMinutes = 3, SortOrder = 2 },
                new CurriculumItem { CourseId = course.Id, Type = "video", Title = "İletişim ve Dinleme", DurationMinutes = 3, SortOrder = 3 },
                new CurriculumItem { CourseId = course.Id, Type = "video", Title = "Çözüm Stratejileri", DurationMinutes = 3, SortOrder = 4 },
                new CurriculumItem { CourseId = course.Id, Type = "video", Title = "Örnek Senaryolar", DurationMinutes = 2, SortOrder = 5 },
                new CurriculumItem { CourseId = course.Id, Type = "pdf", Title = "PDF: Çatışma Rehberi", SortOrder = 6 },
                new CurriculumItem { CourseId = course.Id, Type = "pdf", Title = "PDF: Kontrol Listesi", SortOrder = 7 },
                new CurriculumItem { CourseId = course.Id, Type = "pdf", Title = "PDF: Özet Notlar", SortOrder = 8 },
                new CurriculumItem { CourseId = course.Id, Type = "article", Title = "Makale: Zor İnsanlarla Başa Çıkma", SortOrder = 9 },
                new CurriculumItem { CourseId = course.Id, Type = "article", Title = "Makale: Win-Win İletişim", SortOrder = 10 }
            };
            db.CurriculumItems.AddRange(items);
            await db.SaveChangesAsync();
        }

        if (!await db.HeroSlides.AnyAsync())
        {
            db.HeroSlides.AddRange(
                new HeroSlide { Title = "Başarıya Giden En Hızlı Yol", Text = "Bilgi dolu, hayatın içinden, faydalı ve eğlenceli eğitimlerle kendinizi ve ekibinizi geliştirin. İstediğiniz eğitimi satın alın, aldığınız eğitimlere süresiz erişin.", ImageUrl = "/images/hero-1.jpg", ImageAlt = "Online eğitim ile başarıya giden yol", SortOrder = 1 },
                new HeroSlide { Title = "Öğrenmek Artık Çok Daha Kolay", Text = "Video dersler, PDF kaynaklar ve makalelerle ihtiyacınız olan bilgiye anında ulaşın. Uzman eğitmenlerden, hayatın içinden örneklerle öğrenin — istediğiniz zaman, istediğiniz yerde.", ImageUrl = "/images/hero-2.jpg", ImageAlt = "Öğrenmek artık çok daha kolay", SortOrder = 2 },
                new HeroSlide { Title = "Kendinizi Geliştirin, Fark Yaratın", Text = "Kişisel ve mesleki gelişim yolculuğunuzda yanınızdayız. İletişimden lideliğe, satıştan zaman yönetimine — ihtiyacınız olan her eğitim, bir tık uzağınızda.", ImageUrl = "/images/hero-3.jpg", ImageAlt = "Kendinizi geliştirin, fark yaratın", SortOrder = 3 }
            );
            await db.SaveChangesAsync();
        }
    }
}
