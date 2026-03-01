namespace HizliOgren.Models;

/// <summary>Admin: Üyenin eğitim paketlerini düzenleme sayfası için model.</summary>
public class AdminUserCoursesModel
{
    public int UserId { get; set; }
    public string UserFullName { get; set; } = "";

    /// <summary>Satın alınan eğitimler (mevcut).</summary>
    public List<UserCourseItem> Purchased { get; set; } = new();

    /// <summary>Eklenebilecek eğitimler (henüz alınmamış).</summary>
    public List<CourseItem> AvailableToAdd { get; set; } = new();
}

public class UserCourseItem
{
    public int UserCourseId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
}

public class CourseItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
}
