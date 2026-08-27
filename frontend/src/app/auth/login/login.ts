import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule], // HTML'in formları tanıması ve yönlendirme yapabilmesi için eklendi
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  girilenEmail: string = '';
  girilenSifre: string = '';

  constructor(private router: Router, private http: HttpClient) {}

  sistemeGirisYap() {
    // 1. C# tarafındaki LoginDto'ya uygun olarak değişkenleri isimlendiriyoruz
    const loginData = { 
      Email: this.girilenEmail, 
      Password: this.girilenSifre 
    };

    // 2. Swagger'daki Login URL'sine POST isteği atıyoruz (Port numaran farklıysa 5001'i değiştirmeyi unutma)
    this.http.post<any>('http://localhost:5175/api/auth/login', loginData).subscribe({
      next: (cevap) => {
        // Backend'den dönen Token'ı kaydediyoruz
        localStorage.setItem('aktifKullaniciToken', cevap.token); 
        
        // C# AuthController'dan dönen "Role" bilgisini alıyoruz
        const yetki = cevap.role; 

        // 3. Yetkiye göre sayfalara otomatik yönlendirme
        if (yetki === 'Manager') {
          this.router.navigate(['/manager']);
        } 
        else if (yetki === 'Developer') {
          this.router.navigate(['/developer']);
        } 
        else if (yetki === 'Tester') {
          this.router.navigate(['/tester']);
        }
      },
      error: (hata) => {
        console.error('API Hatası:', hata);
        alert('Giriş başarısız! Lütfen e-posta ve şifrenizi kontrol edin.');
      }
    });
  }
}