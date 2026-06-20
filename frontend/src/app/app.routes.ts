import { Routes } from '@angular/router';
import { Pos } from './pos/pos';
import { Login } from './auth/login';
import { ProductsAdmin } from './admin/products-admin';
import { authGuard, managerGuard } from './auth/guards';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', component: Pos, canActivate: [authGuard] },
  { path: 'admin/products', component: ProductsAdmin, canActivate: [managerGuard] },
  { path: '**', redirectTo: '' }
];
