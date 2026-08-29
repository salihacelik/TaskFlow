import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-developer-panel',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule],
  templateUrl: './developer-panel.html'
})
export class DeveloperPanel implements OnInit {
  
  kullaniciAdi: string = '';
  
  atananGorevler: any[] = [];
  bugRaporlari: any[] = [];
  toplamGorev: number = 0;
  tamamlananGorev: number = 0;

  secilenYorumGorevId: number | null = null;
  yeniYorumMetni: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.kullaniciAdi = localStorage.getItem('aktifKullanici') || 'Saliha';
    this.verileriGetir();
  }

  getHeaders() {
    const token = localStorage.getItem('aktifKullaniciToken');
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

 verileriGetir() {
    this.http.get<any[]>('http://localhost:5175/api/Developer/my-tasks', { headers: this.getHeaders() })
      .subscribe({
        next: (veri) => {
          // C#'tan gelen düz listeyi HTML arayüzünün aradığı yapıya eşliyoruz
          const formatliVeri = veri.map(t => ({
            taskId: t.id || t.taskId,
            description: t.description,
            projectName: 'Sisteme Kayıtlı Proje', // C#'tan düz liste döndüğü için varsayılan atandı
            status: t.status,
            cozuldu: false
          }));

          // Verileri durumlarına göre panellere ayırıyoruz
          this.atananGorevler = formatliVeri.filter(t => t.status !== 'BUG OLUŞTURULDU');
          this.bugRaporlari = formatliVeri.filter(t => t.status === 'BUG OLUŞTURULDU');
          
          // İstatistik kutularını gelen listenin uzunluğuna göre dolduruyoruz
          this.toplamGorev = formatliVeri.length;
          this.tamamlananGorev = formatliVeri.filter(t => t.status === 'TAMAMLANDI' || t.status === 'Tamamlandı').length;
        },
        error: (err) => console.error('Veriler çekilemedi:', err)
      });
  }

  // HTML'in bulamadığı 1. Fonksiyon
  durumlariKaydet() {
    this.atananGorevler.forEach(gorev => {
      const payload = { TaskId: gorev.taskId, NewStatus: gorev.status };
      this.http.put('http://localhost:5175/api/Developer/update-status', payload, { headers: this.getHeaders() }).subscribe();
    });
    alert('Görev durumları başarıyla güncellendi!');
    this.verileriGetir();
  }

  // HTML'in bulamadığı 2. Fonksiyon
  raporlariGuncelle() {
    this.bugRaporlari.forEach(bug => {
      if (bug.cozuldu) {
        const payload = { TaskId: bug.taskId, NewStatus: 'YAPILIYOR' }; 
        this.http.put('http://localhost:5175/api/Developer/update-status', payload, { headers: this.getHeaders() }).subscribe();
      }
    });
    alert('Çözülen hatalar sisteme kaydedildi!');
    setTimeout(() => this.verileriGetir(), 500); 
  }

  // HTML'in bulamadığı 3. Fonksiyon
  yorumGonder() {
    if (!this.secilenYorumGorevId || !this.yeniYorumMetni.trim()) {
      alert('Lütfen görev seçip yorumunuzu yazın.');
      return;
    }

    const payload = { TaskId: Number(this.secilenYorumGorevId), CommentText: this.yeniYorumMetni };
    this.http.post('http://localhost:5175/api/Developer/add-comment', payload, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          alert('Yorum başarıyla eklendi!');
          this.secilenYorumGorevId = null;
          this.yeniYorumMetni = '';
        },
        error: (err) => alert('Yorum eklenirken hata oluştu.')
      });
  }
}