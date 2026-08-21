import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  
  // Router servisini sayfamıza enjekte ediyoruz
  constructor(private router: Router) {}

  // Butona tıklandığında çalışacak fonksiyon
  onRegister() {
    // 1. Kullanıcıya başarılı uyarısı göster
    alert('Kayıt işleminiz başarıyla gerçekleşti! Giriş ekranına yönlendiriliyorsunuz.');
    
    // 2. Uyarıya 'Tamam' dedikten sonra login sayfasına fırlat
    this.router.navigate(['/login']);
  }
}
