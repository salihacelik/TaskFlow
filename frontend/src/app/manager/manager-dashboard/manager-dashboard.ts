import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms'; // Input'tan veri almak için şart!

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule], 
  templateUrl: './manager-dashboard.html'
})
export class ManagerDashboardComponent implements OnInit {
  
  // HTML'nin aradığı ve senin sildiğin o hayati değişkenler:
  kullaniciAdi: string = '';
  toplamProje: number = 0;
  tamamlananProje: number = 0;
  toplamBug: number = 0;
  projeler: any[] = []; 
  
  // Yeni proje eklemek için kullanacağımız değişken
  yeniProjeAdi: string = ''; 

  constructor(private http: HttpClient) {}

  ngOnInit() {
    // Giriş yapan kişiyi yakalıyoruz (Giriş yapılmamışsa varsayılan isim Saliha olur)
    this.kullaniciAdi = localStorage.getItem('aktifKullanici') || 'Saliha (Yönetici)';
    this.veritabanindanVerileriCek();
  }

  // 1. Veritabanından Verileri Okuma (GET)
  veritabanindanVerileriCek() {
    // Dashboard İstatistikleri
    this.http.get<any>('https://localhost:5001/api/manager/dashboard').subscribe({
      next: (veri) => {
        this.toplamProje = veri.toplamProje;
        this.tamamlananProje = veri.tamamlananProje;
        this.toplamBug = veri.toplamBug;
      },
      error: (hata) => console.log('İstatistikler çekilemedi:', hata)
    });

    // Aktif Projeler Listesi
    this.http.get<any[]>('https://localhost:5001/api/manager/projeler').subscribe({
      next: (veri) => this.projeler = veri,
      error: (hata) => console.log('Projeler çekilemedi:', hata)
    });
  }

  // 2. Veritabanına Yeni Proje Yazma (POST)
  projeEkle() {
    // Boş gönderimi engellemek için kontrol
    if (!this.yeniProjeAdi.trim()) {
      alert('Lütfen bir proje adı giriniz!');
      return;
    }

    // C# API'nin beklediği JSON nesnesi
    const eklenecekProje = { 
      ad: this.yeniProjeAdi 
    };

    // SQL'e POST isteği atıyoruz
    this.http.post('https://localhost:5001/api/manager/proje-ekle', eklenecekProje).subscribe({
      next: (cevap) => {
        alert('Harika! Yeni proje SQL veritabanına başarıyla eklendi.');
        this.yeniProjeAdi = ''; // Başarılı olunca input kutusunu temizle
        this.veritabanindanVerileriCek(); // Ekrandaki sayıları ve listeyi güncelle
      },
      error: (hata) => {
        console.error('Proje eklenirken hata oluştu:', hata);
        alert('Veritabanına ekleme yapılamadı. API bağlantınızı kontrol edin.');
      }
    });
  }
}