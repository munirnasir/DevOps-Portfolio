import { Routes } from '@angular/router';
import { Pos } from './pos/pos';

export const routes: Routes = [
  { path: '', component: Pos },
  { path: '**', redirectTo: '' }
];
