import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpClientModule } from '@angular/common/http'; 
import { FormsModule } from '@angular/forms'; // [(ngModel)] çalışması için eklendi

@Component({
  selector: 'app-tester-panel', 
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule], 
  templateUrl: './tester-panel.html'
})
export class TesterPanel implements OnInit {
  
  kullaniciAdi: string = '';
  toplamBug: number = 0; 
  cozulenBug: number = 0; 

  hazirGorevler: any[] = [];

  // Pembe Panel (Test Değerlendirme) Değişkenleri
  secilenTestGorevId: number | null = null;
  testKarari: boolean | null = null; // true: Onayla, false: Bug
  testYorumu: string = '';

  // Mor Panel (Sadece Yorum) Değişkenleri
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
    this.http.get<any>('http://localhost:5175/api/Tester/ready-for-test', { headers: this.getHeaders() })
      .subscribe({
        next: (veri) => {
          this.hazirGorevler = veri.tasks || [];
          this.toplamBug = veri.toplamBug || 0;
          this.cozulenBug = veri.cozulenBug || 0;
        },
        error: (err) => console.error('Veriler çekilemedi:', err)
      });
  }

  testSonucunuSistemeIsle() {
    if (!this.secilenTestGorevId || this.testKarari === null) {
      alert('Lütfen bir görev seçin ve Onayla/Bug Bildir butonlarından birine tıklayın.');
      return;
    }

    if (this.testKarari === false && !this.testYorumu.trim()) {
      alert('Bug bildirirken lütfen hata detayını açıklama kısmına yazın!');
      return;
    }

    const payload = { 
      TaskId: Number(this.secilenTestGorevId), 
      IsSuccessful: this.testKarari,
      CommentText: this.testYorumu 
    };
    
    this.http.put('http://localhost:5175/api/Tester/evaluate-task', payload, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          alert(this.testKarari ? 'Görev başarıyla onaylandı ve kapatıldı.' : 'Görev Bug statüsüne çekilip geliştiriciye iade edildi.');
          this.secilenTestGorevId = null;
          this.testKarari = null;
          this.testYorumu = '';
          this.verileriGetir(); // Ekranı anında günceller
        },
        error: (err) => alert('İşlem sırasında hata oluştu.')
      });
  }

  yorumGonder() {
    if (!this.secilenYorumGorevId || !this.yeniYorumMetni.trim()) {
      alert('Lütfen görev seçip yorumunuzu yazın.');
      return;
    }

    const payload = { TaskId: Number(this.secilenYorumGorevId), CommentText: this.yeniYorumMetni };
    this.http.post('http://localhost:5175/api/Tester/add-comment', payload, { headers: this.getHeaders() })
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