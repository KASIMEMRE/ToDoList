📝 Todo Management System - RESTful API
Bu proje, .NET 8 ekosistemi kullanılarak geliştirilmiş, güvenliği ve ölçeklenebilirliği ön planda tutan bir Yapılacaklar Listesi (Todo) yönetim API'sidir. Önceki CRUD projelerinden farklı olarak, JWT tabanlı kimlik doğrulama ve kullanıcı bazlı yetkilendirme mimarisi üzerine inşa edilmiştir.

🎯 Proje Hedefleri & Kazanımlar
Kimlik Doğrulama: JWT (JSON Web Token) entegrasyonu ile güvenli oturum yönetimi.

Veri Güvenliği: BCrypt ile şifrelerin tek yönlü hash'lenerek saklanması.

Sahiplik Kontrolü: Kullanıcıların sadece kendi oluşturdukları verilere erişebilmesini sağlayan yetkilendirme mantığı.

Mimari Tasarım: DTO (Data Transfer Object) kullanımı ile veritabanı modellerinin soyutlanması.

Modern API Standartları: Sayfalama (Pagination), Filtreleme ve tutarlı HTTP durum kodları.

🛠️ Teknik Yığın (Tech Stack)
Backend: ASP.NET Core Web API (.NET 8)

Veritabanı: MS SQL Server

ORM: Entity Framework Core (Code-First)

Güvenlik: Microsoft.AspNetCore.Authentication.JwtBearer & BCrypt.Net-Next

Test: Postman

🚀 Başlangıç
1. Veritabanı Yapılandırması
appsettings.json dosyasındaki bağlantı dizesini kendi yerel SQL Server bilgilerinizle güncelleyin

2. Migration Uygulama
Terminal üzerinden tabloları oluşturun   -   dotnet ef database update

3. Çalıştırma  -   dotnet run

📡 API Uç Noktaları
🔐 Kimlik Doğrulama (Auth)

🔐 Kimlik Doğrulama (Auth)
POST/api/auth/registerYeni kullanıcı kaydı oluşturur.
POST/api/auth/loginGiriş işlemi ve JWT Token üretimi sağlar.

📝 Görev Yönetimi (Todos) - 
[Authorize]
GET/api/todos Görevleri listeler (Filtre: isCompleted, Sayfalama: page, limit).
POST/api/todosYeni bir görev oluşturur.
PUT/api/todos/{id}Belirli bir görevi günceller (Sahiplik kontrolü yapılır).
DELETE/api/todos/{id}Belirli bir görevi siler (Sahiplik kontrolü yapılır).

🔒 Güvenlik Notları
Tüm Todo istekleri Authorization: Bearer <token> başlığı gerektirir.

Bir kullanıcı, başka bir kullanıcının ID'sini bilse dahi o görevi güncelleyemez veya silemez (403 Forbidden).

Hatalı girişlerde ve yetkisiz erişimlerde standart HTTP 401/400 hataları döndürülür.
