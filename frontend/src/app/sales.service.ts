import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateSale, Sale } from './models';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private readonly http = inject(HttpClient);

  createSale(sale: CreateSale): Observable<Sale> {
    return this.http.post<Sale>('/api/sales', sale);
  }

  getRecentSales(): Observable<Sale[]> {
    return this.http.get<Sale[]>('/api/sales');
  }

  getReceipt(saleId: string): Observable<string> {
    return this.http.get(`/api/sales/${saleId}/receipt`, { responseType: 'text' });
  }
}
