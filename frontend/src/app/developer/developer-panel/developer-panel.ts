import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-developer-panel', // Senin selector adın farklıysa ona dokunma
  standalone: true,
  imports: [CommonModule],
  templateUrl: './developer-panel.html'
})
export class DeveloperPanel implements OnInit {
  
  // HTML'in aradığı ve hata fırlattığı o kritik değişken:
  kullaniciAdi: string = '';

  ngOnInit() {
    // Sisteme giriş yapan geliştiricinin adını hafızadan alıyoruz
    this.kullaniciAdi = localStorage.getItem('aktifKullanici') || 'Saliha';
  }
}