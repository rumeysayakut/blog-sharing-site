# RumyBlog

ASP.NET Core tabanlı, Web API ve kullanıcı arayüzünü (Web UI) bir arada barındıran blog yönetim sistemi.

---

 Proje Hakkında
Bu proje; kullanıcıların kayıt olup giriş yapabildiği, blog gönderileri paylaşabildiği, paylaştığı içerikleri listeleyip güncelleyebildiği ve silebildiği bir web uygulamasıdır.

Uygulama iki ana parçadan oluşmaktadır:
* **RumyBlog.API:** Kimlik doğrulama ve blog gönderi operasyonlarını yöneten RESTful Web API katmanı
* **RumyBlog Web UI:** Kullanıcıların sistemle etkileşime girdiği web arayüzü katmanı

---

##  Temel Özellikler

### Kimlik Doğrulama ve Hesap Yönetimi (Auth / Account)
* Kullanıcı kayıt olma (`Register`) ve giriş yapma (`Login`) işlemleri
* Token yenileme desteği (`/api/auth/refresh-token`)
* Swagger üzerinden JWT / Bearer tabanlı korunan uç noktaları test edebilme olanağı (`Authorize` entegrasyonu)
* Güvenli oturum kapatma (`Çıkış`) mekanizması

### Blog ve Gönderi Yönetimi (Posts)
* **Tüm Bloglar:** Sistemdeki blog gönderilerini başlık, içerik, yazar e-postası ve görseliyle birlikte ana sayfada kart formatında listeleme
* **Blog Paylaşımı:** Başlık, metin içeriği ve yerel dosya seçici üzerinden görsel yükleme desteğiyle yeni blog içeriği oluşturma
* **Benim Bloglarım:** Giriş yapmış kullanıcının yalnızca kendi oluşturduğu blog yazılarını görüntüleyebildiği özel yönetim paneli
* **Gönderi Güncelleme:** Önceden paylaşılan blog başlığı ve içeriğini form üzerinden düzenleyebilme
* **Gönderi Silme:** Blog gönderilerini sistemden kaldırabilme

---



##  Kullanılan Teknolojiler

* **C# / .NET** — Uygulama geliştirme platformu
* **ASP.NET Core Web API** — RESTful HTTP servisleri ve iş mantığı uç noktaları[cite: 1]
* **ASP.NET Core MVC / Razor** — Sayfa yönlendirmeleri (`/Account/Login`, `/Home/Index`, `/Posts/PostAl/Create`, `/Posts/PostMy/Posts`) ve web arayüzü bileşenleri[cite: 1]
* **Swagger / OpenAPI 3.0** — API dokümantasyonu ve uç nokta test arayüzü[cite: 1]
* **HTML5 & CSS3** — Arayüz düzeni ve responsive kart/form tasarımları[cite: 1]

---

##  Kurulum ve Çalıştırma

1. Depoyu yerel bilgisayarınıza klonlayın:
   ```bash
   git clone [https://github.com/kullanici-adiniz/RumyBlog.git](https://github.com/kullanici-adiniz/RumyBlog.git)
   cd RumyBlog

   Çözüm dosyasını (.sln) Visual Studio veya tercih ettiğiniz bir IDE ile açın.

appsettings.json dosyası içerisindeki veritabanı bağlantı dizesini (ConnectionStrings) kendi yerel ortamınıza göre yapılandırın.

Bağımlılıkları yükleyin ve veritabanı şemasını uygulayın:

Bash
dotnet restore
dotnet ef database update
Projeyi başlatın:

Bash
dotnet run
Tarayıcı üzerinden uygulamaya ve Swagger arayüzüne erişin:

Web Arayüzü: https://localhost:44575/Account/Login



Swagger UI: https://localhost:44575/swagger/index.html
