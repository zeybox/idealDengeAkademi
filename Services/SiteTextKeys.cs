namespace HizliOgren.Services;

/// <summary>Site sabit yazıları için Settings tablosundaki anahtarlar ve varsayılan değerler.</summary>
public static class SiteTextKeys
{
    public const string AboutPageContent = "AboutPageContent";

    public const string Feature1Title = "Feature1Title";
    public const string Feature1Text = "Feature1Text";
    public const string Feature2Title = "Feature2Title";
    public const string Feature2Text = "Feature2Text";
    public const string Feature3Title = "Feature3Title";
    public const string Feature3Text = "Feature3Text";

    public const string SectionEgitimlerTitle = "SectionEgitimlerTitle";
    public const string SectionEgitimlerSubtitle = "SectionEgitimlerSubtitle";
    public const string SectionCtaTitle = "SectionCtaTitle";
    public const string SectionCtaText = "SectionCtaText";
    public const string SectionBlogTitle = "SectionBlogTitle";
    public const string SectionBlogSubtitle = "SectionBlogSubtitle";
    public const string SectionHakkimizdaTitle = "SectionHakkimizdaTitle";
    public const string SectionHakkimizdaSubtitle = "SectionHakkimizdaSubtitle";
    public const string SectionHakkimizdaButton = "SectionHakkimizdaButton";

    public const string FooterTagline = "FooterTagline";
    public const string FooterCopyright = "FooterCopyright";

    public const string MetaDescriptionDefault = "MetaDescriptionDefault";

    public const string HakkimizdaBreadcrumbSubtitle = "HakkimizdaBreadcrumbSubtitle";
    public const string HakkimizdaBoxTitle = "HakkimizdaBoxTitle";
    public const string HakkimizdaBoxText = "HakkimizdaBoxText";

    public static readonly string[] AllKeys =
    {
        AboutPageContent,
        Feature1Title, Feature1Text, Feature2Title, Feature2Text, Feature3Title, Feature3Text,
        SectionEgitimlerTitle, SectionEgitimlerSubtitle,
        SectionCtaTitle, SectionCtaText,
        SectionBlogTitle, SectionBlogSubtitle,
        SectionHakkimizdaTitle, SectionHakkimizdaSubtitle, SectionHakkimizdaButton,
        FooterTagline, FooterCopyright,
        MetaDescriptionDefault,
        HakkimizdaBreadcrumbSubtitle, HakkimizdaBoxTitle, HakkimizdaBoxText
    };

    /// <summary>Site Sabit Yazılar formunda düzenlenen anahtarlar (AboutPageContent hariç).</summary>
    public static readonly string[] FormKeys = AllKeys.Where(k => k != AboutPageContent).ToArray();

    public static string GetDefault(string key)
    {
        return key switch
        {
            Feature1Title => "1000+ Mikro Eğitim",
            Feature1Text => "Soru ve cevaplarla farkındalığınızı artırın. Video, PDF ve makalelerle öğrenin.",
            Feature2Title => "Uzmanlaşın",
            Feature2Text => "Alanında uzman eğitmenlerden doğru eğitimi alın. Uygulanabilir, güncel içerikler.",
            Feature3Title => "Kolay Erişim",
            Feature3Text => "Eğitimlerinize ister bilgisayardan ister mobil cihazdan erişin.",
            SectionEgitimlerTitle => "Eğitimler",
            SectionEgitimlerSubtitle => "Popüler eğitimlerimizi keşfedin ve hemen öğrenmeye başlayın.",
            SectionCtaTitle => "Eğitim Satın Al, Hemen Eriş",
            SectionCtaText => "İstediğiniz eğitimi satın alın; video, PDF ve makalelere süresiz erişin. Aldığınız her eğitim hesabınızda kalır.",
            SectionBlogTitle => "Blog",
            SectionBlogSubtitle => "İpuçları, makaleler ve güncel yazılar. Herkese açık.",
            SectionHakkimizdaTitle => "Hakkımızda",
            SectionHakkimizdaSubtitle => "Hızlı Öğren, kişisel ve mesleki gelişim için tasarlanmış bir online eğitim platformudur. Alanında uzman eğitmenlerimizle hazırlanan video, PDF ve makalelerle kendinize yeni yetkinlikler kazandırın.",
            SectionHakkimizdaButton => "Hakkımızda Sayfası",
            FooterTagline => "Öğrenmek hiç bu kadar hızlı olmamıştı. Hemen başlayın — ilk eğitiminiz bir tık uzağınızda.",
            FooterCopyright => "© {0} Hızlı Öğren (www.hizliogren.net.tr). Tüm hakları saklıdır.",
            MetaDescriptionDefault => "Hızlı Öğren ile online eğitimlere katılın. Video, PDF ve makalelerle kişisel ve mesleki gelişim.",
            HakkimizdaBreadcrumbSubtitle => "Hızlı Öğren olarak nereden geldiğimiz, neye inandığımız ve sizin için neler sunduğumuz.",
            HakkimizdaBoxTitle => "Hemen Başlayın",
            HakkimizdaBoxText => "Eğitim kataloğumuzu inceleyin, ihtiyacınıza uygun eğitimi seçin ve öğrenmeye bugün başlayın.",
            _ => ""
        };
    }
}
