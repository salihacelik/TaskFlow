import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms'; 

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule], 
  templateUrl: './manager-dashboard.html'
})
export class ManagerDashboardComponent implements OnInit {
  
  kullaniciAdi: string = '';
  toplamProje: number = 0;
  tamamlananProje: number = 0;
  toplamBug: number = 0;
  
  projeler: any[] = []; 
  gelistiriciler: any[] = []; 
  
  // Proje Ekleme Değişkeni
  yeniProjeAdi: string = ''; 

  // Görev Atama Değişkenleri
  secilenProjeId: number | null = null;
  secilenGelistiriciId: number | null = null;
  gorevAciklamasi: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.kullaniciAdi = localStorage.getItem('aktifKullanici') || 'Saliha (Yönetici)';
    this.veritabanindanVerileriCek();
  }

  veritabanindanVerileriCek() {
    const token = localStorage.getItem('aktifKullaniciToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    this.http.get<any>('http://localhost:5175/api/Manager/dashboard', { headers }).subscribe({
      next: (veri) => {
        this.toplamProje = veri.statistics.projects.total;
        this.tamamlananProje = veri.statistics.projects.completed;
        this.toplamBug = veri.statistics.tasks.totalBugs;
        
        this.projeler = veri.projectsOverview; 
        this.gelistiriciler = veri.team.developers; 
      },
      error: (hata) => console.log('Dashboard verileri çekilemedi:', hata)
    });
  }

  projeEkle() {
    if (!this.yeniProjeAdi.trim()) {
      alert('Lütfen bir proje adı giriniz!');
      return;
    }

    const token = localStorage.getItem('aktifKullaniciToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    
    const eklenecekProje = { 
      ProjectName: this.yeniProjeAdi,
      Description: 'Yönetici paneli üzerinden eklendi.'
    };

    this.http.post('http://localhost:5175/api/Manager/create-project', eklenecekProje, { headers }).subscribe({
      next: () => {
        alert('Harika! Yeni proje başarıyla oluşturuldu.');
        this.yeniProjeAdi = ''; 
        this.veritabanindanVerileriCek(); 
      },
      error: (hata) => {
        console.error('Proje eklenirken hata oluştu:', hata);
        alert('Veritabanına ekleme yapılamadı.');
      }
    });
  }

  // YENİ EKLENEN GÖREV ATAMA FONKSİYONU
  gorevAta() {
    if (!this.secilenProjeId || !this.secilenGelistiriciId || !this.gorevAciklamasi.trim()) {
      alert('Lütfen proje ve geliştirici seçip, görev açıklamasını yazın!');
      return;
    }

    const token = localStorage.getItem('aktifKullaniciToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    const gidenGorev = {
      ProjectId: Number(this.secilenProjeId),
      DeveloperId: Number(this.secilenGelistiriciId),
      Description: this.gorevAciklamasi
    };

    this.http.post('http://localhost:5175/api/Manager/assign-task', gidenGorev, { headers }).subscribe({
      next: () => {
        alert('Görev, seçilen geliştiriciye ATANDI statüsüyle başarıyla iletildi!');
        this.secilenProjeId = null;
        this.secilenGelistiriciId = null;
        this.gorevAciklamasi = '';
        this.veritabanindanVerileriCek(); 
      },
      error: (hata) => {
        console.error('Görev atama hatası:', hata);
        alert('Görev atanamadı. Bilgileri kontrol edin.');
      }
    });
  }
}