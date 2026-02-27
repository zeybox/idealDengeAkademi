using Microsoft.AspNetCore.Mvc.Rendering;

namespace HizliOgren.Helpers;

/// <summary>Türkiye'deki 81 ili içerir; formlarda şehir dropdown için kullanılır.</summary>
public static class TurkishCities
{
    private static readonly string[] Cities =
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Aksaray", "Amasya", "Ankara", "Antalya",
        "Ardahan", "Artvin", "Aydın", "Balıkesir", "Bartın", "Batman", "Bayburt", "Bilecik",
        "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum",
        "Denizli", "Diyarbakır", "Düzce", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir",
        "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Iğdır", "Isparta", "İstanbul",
        "İzmir", "Kahramanmaraş", "Karabük", "Karaman", "Kars", "Kastamonu", "Kayseri", "Kilis",
        "Kırıkkale", "Kırklareli", "Kırşehir", "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa",
        "Mardin", "Mersin", "Muğla", "Muş", "Nevşehir", "Niğde", "Ordu", "Osmaniye",
        "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Şanlıurfa", "Şırnak",
        "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Uşak", "Van", "Yalova", "Yozgat", "Zonguldak"
    };

    /// <summary>Şehir dropdown için SelectList döner; ilk öğe boş (seçmek istemiyorum) seçeneğidir.</summary>
    public static SelectList GetCitySelectList(string? selectedValue = null)
    {
        var items = new List<SelectListItem> { new SelectListItem("Seçiniz", "", string.IsNullOrEmpty(selectedValue)) };
        foreach (var city in Cities)
            items.Add(new SelectListItem(city, city, string.Equals(city, selectedValue, StringComparison.OrdinalIgnoreCase)));
        return new SelectList(items, "Value", "Text", selectedValue);
    }
}
