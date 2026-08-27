import { Routes } from '@angular/router';

export const routes: Routes = [
  // 1. VARSAYILAN ROTA VE KİMLİK DOĞRULAMA
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./auth/login/login').then(m => m.Login) 
  },    
  { 
    path: 'register', 
    loadComponent: () => import('./auth/register/register').then((m: any) => m.RegisterComponent ) 
  },

  // 2. YETKİYE GÖRE YÖNLENDİRME KAVŞAĞI
  
  // Manager (Yönetici) Rotası
  { 
    path: 'manager', 
    loadComponent: () => import('./manager/manager-dashboard/manager-dashboard').then(m => m.ManagerDashboardComponent)
  },

  // Developer (Geliştirici) Rotası
  { 
    path: 'developer', 
    loadComponent: () => import('./developer/developer-panel/developer-panel').then(m => m.DeveloperPanel) 
  },

  // Tester (Test Uzmanı) Rotası
  { 
    path: 'tester', 
    loadComponent: () => import('./tester/tester-panel/tester-panel').then(m => m.TesterPanel) 
  }
];