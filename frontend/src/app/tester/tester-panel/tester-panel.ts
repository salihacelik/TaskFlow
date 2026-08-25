import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http'; // API istekleri için eklendi

@Component({
  selector: 'app-tester-panel', 
  standalone: true,
  imports: [CommonModule, HttpClientModule], // HttpClientModule buraya eklendi
  templateUrl: './tester-panel.html'
})
export class TesterPanel implements OnInit {
  
  kullaniciAdi: string = '';
  toplamBug: number = 24; 
  cozulenBug: number = 18; 

  // Backend ile konuşabilmek için http servisini inşa ediyoruz
  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.kullaniciAdi = localStorage.getItem('aktifKullanici') || 'Saliha';
  }

  //karar 
  testSonucunuSistemeIsle(gorevId: number, testBasariliMi: boolean, hataDetayi: string) {
    if (testBasariliMi === true) {
      const guncelData = { durum: 'Completed', yorum: 'Test Başarılı. ' + hataDetayi };
      
      this.http.put(`https://localhost:5001/api/tester/onayla/${gorevId}`, guncelData)
        .subscribe(res => alert('Görev başarıyla onaylandı ve kapatıldı.'));
    } 
    else {
      if (!hataDetayi.trim()) {
        alert('Lütfen bug detayını yoruma ekleyiniz!');
        return;
      }
      
      const bugData = { durum: 'Bug Oluşturuldu', yorum: hataDetayi };
      
      this.http.put(`https://localhost:5001/api/tester/bug-bildir/${gorevId}`, bugData)
        .subscribe(res => {
          alert('Görev Bug statüsüne çekilip geliştiriciye iade edildi.');
          this.toplamBug += 1; 
        });
    }
  }
}