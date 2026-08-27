import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent {
  girilenKullaniciAdi: string = '';
  girilenEmail: string = '';
  girilenSifre: string = '';
  secilenRol: string = 'Manager';

  constructor(private router: Router, private http: HttpClient) {}

  onRegister() {
    const registerData = {
      username: this.girilenKullaniciAdi,
      email: this.girilenEmail,
      password: this.girilenSifre,
      role: this.secilenRol
    };

    this.http.post('http://localhost:5175/api/auth/register', registerData).subscribe({
      next: () => {
        alert('Kayıt işleminiz başarıyla gerçekleşti! Giriş ekranına yönlendiriliyorsunuz.');
        this.router.navigate(['/login']);
      },
      error: (hata) => {
        console.error('API Hatası:', hata);
        alert('Kayıt işlemi başarısız! Lütfen bilgilerinizi kontrol edin.');
      }
    });
  }
}
