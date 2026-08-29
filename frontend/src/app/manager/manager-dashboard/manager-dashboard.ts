import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manager', // Routing'deki adınla eşleşmeli
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './manager-dashboard.html'
})
export class ManagerComponent implements OnInit {
  kullaniciAdi: string = 'Yönetici';
  toplamProje: number = 0;
  tamamlananProje: number = 5; // Örnek statik veri
  toplamBug: number = 12; // Örnek statik veri
  
  projeler: any[] = []; 
  gelistiriciler: any[] = [];
  yeniProjeAdi: string = ''; 

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.veritabanindanVerileriCek();
  }

  // 1. Veritabanından Verileri Okuma (GET)
  veritabanindanVerileriCek() {
    // 401 Hatası almamak için girişte kaydettiğimiz Token'ı C#'a gönderiyoruz
    const token = localStorage.getItem('aktifKullaniciToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    this.http.get<any>('https://localhost:5175/api/manager/dashboard', { headers }).subscribe({
      next: (veri) => {
        // C# DTO'sundan gelen verileri değişkenlerimize aktarıyoruz
        this.projeler = veri.projectsOverview;
        this.gelistiriciler = veri.team.developers;
        this.toplamProje = this.projeler.length; // Proje sayısını otomatik hesapla
      },
      error: (hata) => console.error('Veriler çekilemedi:', hata)
    });
  }

  // 2. Veritabanına Yeni Proje Yazma (POST)
  projeEkle() {
    if (!this.yeniProjeAdi.trim()) {
      alert('Lütfen bir proje adı giriniz!');
      return;
    }

    const token = localStorage.getItem('aktifKullaniciToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    const eklenecekProje = { 
      ProjectName: this.yeniProjeAdi,
      Description: 'Yeni Proje'
    };

    // Eğer backend'de bu endpoint hazır değilse 404 verebilir, hazırsa eklenecektir
    this.http.post('https://localhost:5175/api/manager/proje-ekle', eklenecekProje, { headers }).subscribe({
      next: () => {
        alert('Harika! Yeni proje başarıyla eklendi.');
        this.yeniProjeAdi = ''; 
        this.veritabanindanVerileriCek(); 
      },
      error: (hata) => {
        console.error('Proje eklenirken hata oluştu:', hata);
        alert('Sistem mesajı: C# tarafında proje ekleme (POST) fonksiyonu henüz yazılmamış olabilir.');
      }
    });
  }
}