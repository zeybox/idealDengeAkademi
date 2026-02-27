using System.Text.Json;

namespace HizliOgren.Services;

public class CartService
{
    private const string SessionKey = "Cart_CourseIds";
    private readonly IHttpContextAccessor _http;

    public CartService(IHttpContextAccessor http) => _http = http;

    private List<int> GetIds()
    {
        var session = _http.HttpContext?.Session;
        if (session == null) return new List<int>();
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<int>();
        try
        {
            var list = JsonSerializer.Deserialize<List<int>>(json);
            return list ?? new List<int>();
        }
        catch { return new List<int>(); }
    }

    private void SetIds(List<int> ids)
    {
        var session = _http.HttpContext?.Session;
        if (session == null) return;
        session.SetString(SessionKey, JsonSerializer.Serialize(ids));
    }

    public int Count => GetIds().Count;

    public IReadOnlyList<int> CourseIds => GetIds();

    public void Add(int courseId)
    {
        var ids = GetIds();
        if (!ids.Contains(courseId)) { ids.Add(courseId); SetIds(ids); }
    }

    public void Remove(int courseId)
    {
        var ids = GetIds();
        ids.Remove(courseId);
        SetIds(ids);
    }

    public void Clear() => SetIds(new List<int>());
}
