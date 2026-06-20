import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateSale, PagedResult, Sale } from './models';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private readonly http = inject(HttpClient);

  createSale(sale: CreateSale): Observable<Sale> {
    return this.http.post<Sale>('/api/sales', sale);
  }

  getRecentSales(page = 1, pageSize = 20): Observable<PagedResult<Sale>> {
    return this.http.get<PagedResult<Sale>>('/api/sales', {
      params: { page: String(page), pageSize: String(pageSize) }
    });
  }

  getReceipt(saleId: string): Observable<string> {
    return this.http.get(`/api/sales/${saleId}/receipt`, { responseType: 'text' });
  }
}
